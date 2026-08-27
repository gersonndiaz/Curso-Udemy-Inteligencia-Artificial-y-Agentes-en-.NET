using BlazorAI.Dto;
using BlazorAI.Helpers;
using BlazorAI.RAG.Services;
using BlazorAI.Services.Chatbots;
using Microsoft.Extensions.AI;

namespace BlazorAI.RAG.Chatbots;

public class ChatbotRag : IChatbot
{
    private string model;
    private readonly IChatClientFactory client;
    private readonly ChatOptions chatOptions;
    private readonly IRagService ragService;
    private readonly List<ChatMessage> messages = [];
    private readonly Queue<ToolApprovalRequestContent> approvalPending = new();
    private CancellationTokenSource _ctsCurrent;

    public List<MessageChatUI> Conversation { get; } = [];
    public bool IsProcessing { get; private set; }
    public RequestApprovalUI RequestApprovalPending { get; private set; }
    public event Action? OnChange;

    public ChatbotRag(IChatClientFactory client, ChatOptions chatOptions, IRagService ragService)
    {
        model = ModelsAI.GetModelDefault;
        this.client = client;
        this.chatOptions = chatOptions;
        this.ragService = ragService;
        string systemPrompt = """
                    Eres un asistente especializado exclusivamente en responder preguntas usando el contexto recuperado de documentos internos.

                    Debes responder en español.
                    Las respuestas deben ser en texto plano, sin markdown.

                    Reglas obligatorias:
                    - Responde únicamente con información contenida en el contexto recuperado.
                    - Si la respuesta no está explícitamente en el contexto, debes responder: "No tengo información suficiente en los documentos para responder esa pregunta."
                    - No uses conocimiento general del modelo.
                    - No inventes información.
                    - No respondas preguntas de programación, cultura general, matemáticas u otros temas si no aparecen en el contexto recuperado.
                    - Si la pregunta no está relacionada con los documentos, recházala de forma breve.
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

    public /*async*/ Task ResolveApprovalAsync(bool approved, CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;

        // PARA PROPOSITOS DE PRUEBA, NO SOLICITAREMOS APROBACIÓN HUMANA POR AHORA, POR ESO SE ANTEPONE UN RETURN
        // if (RequestApprovalPending is null || IsProcessing)
        // {
        //     return;
        // }

        // IsProcessing = true;
        // _ctsCurrent = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

        // try
        // {
        //     var approvalResponse = RequestApprovalPending.ToolApprovalRequest.CreateResponse(approved);
        //     messages.Add(new ChatMessage(ChatRole.User, [approvalResponse]));
        //     RequestApprovalPending = null;

        //     Conversation.Add(new MessageChatUI
        //     {
        //         Role = MessageRole.System,
        //         Text = approved ? "Acción aprobada por el usuario" : "Acción rechazada por el usuario"
        //     });

        //     RequestApprovalPending = null;
        //     ShowNextApprovalPending();

        //     if (RequestApprovalPending is not null)
        //     {
        //         IsProcessing = false;
        //         ChangeNotification();
        //         return;
        //     }

        //     Conversation.Add(new MessageChatUI
        //     {
        //         Role = MessageRole.AI,
        //         Text = string.Empty
        //     });

        //     ChangeNotification();
        //     await ProcessResponse(_ctsCurrent.Token);
        // }
        // catch(OperationCanceledException)
        // {
        //     OperationCanceled();
        // }
        // finally
        // {
        //     OperationFinally();
        // }
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
            await ProcessResponse(userText, _ctsCurrent.Token);
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

    private async Task ProcessResponse(string userText, CancellationToken cancellationToken)
    {
        var context = await ragService.SearchContext(userText, top: 3, minScore: 0.6f, cancellationToken);

        // Si no hay contexto, evitamos llamar a la IA, ya que no tiene sentido
        if (!context.Any())
        {
            Conversation[^1].Text = "No tengo información suficiente en los documentos para responder esa pregunta.";
            ChangeNotification();
            return;
        }

        var delimitadorFuentes = "|";

        /*
        Contexto recuperado de la base documental:

        Documento: documento 1
        Contenido: el contenido

        -------

        Documento: documento 2
        Contenido: el contenido del documento 2
        */
        var messageContext = new ChatMessage(ChatRole.System,
            $$"""
            Contexto recuperado de la base documental:
            {{string.Join("\n\n---\n\n", context)}}

            Pregunta del usuario:
            {{userText}}

            Instrucción:
                 - Responde solo si la respuesta está explícitamente respaldada por el contexto recuperado.
                - Si no lo está, responde exactamente:
                    "No tengo información suficiente en los documentos para responder esa pregunta."
                - Primero escribe solamente la respuesta para el usuario, en texto plano.
                - Luego escribe en una nueva línea exactamente:
                    {{delimitadorFuentes}}
                - Después del delimitador, escribe un JSON válido con este formato:
                    {"fuentesUsadas":["Documento-1", "Documento-2"]}
                - Por ejemplo: El nombre del documento se encuentra así "manual-de-politicas-internas.md" donde manual-de-politicas-internas.md sería el título que debes colocar en fuentesUsadas.
                - En "fuentesUsadas" incluye solamente los títulos de documento de las fuentes realmente utilizadas.
                - No incluyas fuentes irrelevantes.
            """
        );

        var messagesToSend = new List<ChatMessage>();
        messagesToSend.AddRange(messages);
        messagesToSend.Insert(messages.Count - 1, messageContext);

        var updates = new List<ChatResponseUpdate>();
        var functionResults = new List<string>();

        var mClient = client.Create(model);

        await foreach(var update in mClient.GetStreamingResponseAsync(messagesToSend, chatOptions, cancellationToken: cancellationToken))
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
