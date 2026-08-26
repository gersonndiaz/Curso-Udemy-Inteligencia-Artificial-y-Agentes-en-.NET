using BlazorAI.Dto;
using BlazorAI.Helpers;
using Microsoft.Extensions.AI;

namespace BlazorAI.Services.Chatbots;

public class Chatbot : IChatbot
{
    private string model;
    private readonly IChatClientFactory client;
    private readonly ChatOptions chatOptions;
    private readonly List<ChatMessage> messages = [];
    private readonly Queue<ToolApprovalRequestContent> approvalPending = new();
    private CancellationTokenSource _ctsCurrent;

    public List<MessageChatUI> Conversation { get; } = [];
    public bool IsProcessing { get; private set; }
    public RequestApprovalUI RequestApprovalPending { get; private set; }
    public event Action? OnChange;

    public Chatbot(IChatClientFactory client, ChatOptions chatOptions)
    {
        model = ModelsAI.GetModelDefault;
        this.client = client;
        this.chatOptions = chatOptions;

        string systemPrompt = """
                    Eres un asistente de inteligencia artificial llamado "Agente Ollama". Tu tarea es ayudar a los usuarios a responder preguntas y proporcionar información útil.
                    Debes responder en Español
                    Las respuestas deben ser claras, concisas y fáciles de entender. Evita dar respuestas vagas o ambiguas.
                    Las respuestas deben ser en texto plano, no usar formatos como markdown. Excepto si es explicitamente solicitada por el usuario, aunque se debe vigilar y bloquear posible código malicioso, sospechoso o mal intencionado.
                    La información mmostrada debe ser entendible por la persona y se deben excluir ID de alguna base de datos o identificadores de base de datos. Se puede mostrar formatos como json solo para casos solicitados, considerando que el usuario puede o no ser desarrollador.
                """ ;

        messages.Add(new ChatMessage(ChatRole.System, systemPrompt));
    }

    public void CancelCurrentResponse()
    {
        if (IsProcessing)
        {
            _ctsCurrent?.Cancel();
        }
    }

    public async Task ResolveApprovalAsync(bool approved, CancellationToken cancellationToken = default)
    {
        if (RequestApprovalPending is null || IsProcessing)
        {
            return;
        }

        IsProcessing = true;
        _ctsCurrent = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

        try
        {
            var approvalResponse = RequestApprovalPending.ToolApprovalRequest.CreateResponse(approved);
            messages.Add(new ChatMessage(ChatRole.User, [approvalResponse]));
            RequestApprovalPending = null;

            Conversation.Add(new MessageChatUI
            {
                Role = MessageRole.System,
                Text = approved ? "Acción aprobada por el usuario" : "Acción rechazada por el usuario"
            });

            RequestApprovalPending = null;
            ShowNextApprovalPending();

            if (RequestApprovalPending is not null)
            {
                IsProcessing = false;
                ChangeNotification();
                return;
            }

            Conversation.Add(new MessageChatUI
            {
                Role = MessageRole.AI,
                Text = string.Empty
            });

            ChangeNotification();
            await ProcessResponse(_ctsCurrent.Token);
        }
        catch(OperationCanceledException)
        {
            OperationCanceled();
        }
        finally
        {
            OperationFinally();
        }
    }

    public async Task SendMessageAsync(string userText, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(userText))
        {
            return;
        }

        if (IsProcessing || RequestApprovalPending is not null)
        {
            return;
        }

        try
        {
            IsProcessing = true;
            _ctsCurrent = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

            Conversation.Add(new MessageChatUI
            {
            Role = MessageRole.User,
            Text = userText 
            });

            messages.Add(new ChatMessage(ChatRole.User, userText));

            Conversation.Add(new MessageChatUI
            {
            Role = MessageRole.AI,
            Text = string.Empty 
            });

            ChangeNotification();
            await ProcessResponse(_ctsCurrent.Token);
        }
        catch (OperationCanceledException)
        {
            OperationCanceled();
        }
        finally
        {
            OperationFinally();
        }
    }

    private void OperationCanceled()
    {
        if (Conversation is not null && Conversation.Count > 0 && Conversation[^1].Role == MessageRole.AI)
        {
            if (string.IsNullOrWhiteSpace(Conversation[^1].Text))
            {
                Conversation[^1].Text = "[Respuesta cancelada]";
            }
            else
            {
                Conversation[^1].Text = "[cancelado]";
            }
        }
    }

    private void OperationFinally()
    {
        _ctsCurrent?.Dispose();
        _ctsCurrent = null;
        IsProcessing = false;
        ChangeNotification();
    }

    private async Task ProcessResponse(CancellationToken cancellationToken)
    {
        var updates = new List<ChatResponseUpdate>();
        var functionResults = new List<string>();

        var mClient = client.Create(model);

        await foreach(var update in mClient.GetStreamingResponseAsync(messages, chatOptions, cancellationToken: cancellationToken))
        {
            updates.Add(update);

            foreach(var content in update.Contents)
            {
                if (content is TextContent textContent)
                {
                    Conversation[^1].Text += textContent.Text;
                    ChangeNotification();
                }
                else if (content is FunctionResultContent functionResult &&
                         functionResult.Result is not null)
                {
                    functionResults.Add(functionResult.Result.ToString()!);
                }
            }
        }

        var response = updates.ToChatResponse();
        messages.AddMessages(response);

        // foreach (var functionResult in functionResults)
        // {
        //     Conversation.Add(new MessageChatUI
        //     {
        //         Role = MessageRole.System,
        //         Text = functionResult
        //     });
        // }

        if (functionResults.Count > 0)
        {
            ChangeNotification();
        }

        var approvalRequests = response.Messages.SelectMany(m => m.Contents).OfType<ToolApprovalRequestContent>().ToList();

        if (approvalRequests is not null && approvalRequests.Count > 0)
        {
            foreach(var request in approvalRequests)
            {
                approvalPending.Enqueue(request);
            }
            
            // Removemos mensaje vacio de la IA
            if (string.IsNullOrWhiteSpace(Conversation[^1].Text))
            {
                Conversation.RemoveAt(Conversation.Count - 1);
            }

            ShowNextApprovalPending();

            ChangeNotification();
            return;
        }
    }

    private void ShowNextApprovalPending()
    {
        if (approvalPending is not null && approvalPending.Count == 0)
        {
            RequestApprovalPending = null;
            return;
        }

        var approvalRequest = approvalPending.Dequeue();

        if (approvalRequest.ToolCall is FunctionCallContent functionCall)
        {
            RequestApprovalPending = new RequestApprovalUI
            {
                ToolApprovalRequest = approvalRequest,
                ToolName = ConvertFunctionName(functionCall.Name),
                Arguments = functionCall.Arguments?.ToDictionary(x => x.Key, x => x.Value) ?? []
            };
        }
    }

    private void ChangeNotification() => OnChange?.Invoke();

    private static string ConvertFunctionName(string name)
    {
        return name switch
        {
            "SendEmailAsync" => "Enviar correo",
            _ => name
        };
    }

    public void SetModel(string model)
    {
        this.model = model;
    }
}
