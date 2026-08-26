using BlazorAI.Dto;
using Microsoft.Extensions.AI;

namespace BlazorAI.Services.Chatbots;

public class Chatbot : IChatbot
{
    private readonly IChatClient client;
    private readonly List<ChatMessage> messages = [];

    public List<MessageChatUI> Conversation { get; } = [];
    public bool IsProcessing { get; private set; }
    public RequestApprovalUI RequestApprovalPending { get; private set; }
    public event Action? OnChange;

    public Chatbot(IChatClient client)
    {
        this.client = client;

        string systemPrompt = """
                    Eres un asistente de inteligencia artificial llamado "Agente Ollama". Tu tarea es ayudar a los usuarios a responder preguntas y proporcionar información útil.
                    Debes responder en Español
                    Las respuestas deben ser claras, concisas y fáciles de entender. Evita dar respuestas vagas o ambiguas.
                    Las respuestas deben ser en texto plano, no usar formatos como markdown.
                """ ;

        messages.Add(new ChatMessage(ChatRole.System, systemPrompt));
    }

    public void CancelCurrentResponse()
    {
    }

    public async Task ResolveApprovalAsync(bool approved, CancellationToken cancellationToken = default)
    {
        if (RequestApprovalPending is null || IsProcessing)
        {
            return;
        }

        IsProcessing = true;
        var approvalResponse = RequestApprovalPending.ToolApprovalRequest.CreateResponse(approved);
        messages.Add(new ChatMessage(ChatRole.User, [approvalResponse]));
        RequestApprovalPending = null;

        Conversation.Add(new MessageChatUI
        {
            Role = MessageRole.System,
            Text = approved ? "Acción aprobada por el usuario" : "Acción rechazada por el usuario"
        });

        Conversation.Add(new MessageChatUI
        {
            Role = MessageRole.AI,
            Text = string.Empty
        });

        ChangeNotification();
        await ProcessResponse(cancellationToken);
        IsProcessing = false;
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

        IsProcessing = true;

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
        await ProcessResponse(cancellationToken);
        IsProcessing = false;
    }

    private async Task ProcessResponse(CancellationToken cancellationToken)
    {
        var updates = new List<ChatResponseUpdate>();
        var functionResults = new List<string>();

        await foreach(var update in client.GetStreamingResponseAsync(messages, cancellationToken: cancellationToken))
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

        foreach (var functionResult in functionResults)
        {
            Conversation.Add(new MessageChatUI
            {
                Role = MessageRole.System,
                Text = functionResult
            });
        }

        if (functionResults.Count > 0)
        {
            ChangeNotification();
        }

        var approvalRequest = response.Messages.SelectMany(m => m.Contents).OfType<ToolApprovalRequestContent>().FirstOrDefault();

        if (approvalRequest is not null)
        {
            if (approvalRequest.ToolCall is FunctionCallContent functionCall)
            {
                RequestApprovalPending = new RequestApprovalUI
                {
                    ToolApprovalRequest = approvalRequest,
                    ToolName = ConvertFunctionName(functionCall.Name),
                    Arguments = functionCall.Arguments?.ToDictionary(x => x.Key, x => x.Value) ?? []
                };
            }
            
            // Removemos mensaje vacio de la IA
            if (string.IsNullOrWhiteSpace(Conversation[^1].Text))
            {
                Conversation.RemoveAt(Conversation.Count - 1);
            }

            ChangeNotification();
            return;
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
}
