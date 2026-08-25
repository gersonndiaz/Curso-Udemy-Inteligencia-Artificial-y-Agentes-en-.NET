using BlazorAI.Dto;
using Microsoft.Extensions.AI;

namespace BlazorAI.Services.Chatbots;

public class Chatbot : IChatbot
{
    private readonly IChatClient client;
    private readonly List<ChatMessage> messages = [];

    public List<MessageChatUI> Conversation { get; } = [];
    public bool IsProcessing { get; private set; }
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
    }

    public async Task SendMessageAsync(string userText, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(userText))
        {
            return;
        }

        if (IsProcessing)
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
            }

            var response = updates.ToChatResponse();
            messages.AddMessages(response);
        }
    }

    private void ChangeNotification() => OnChange?.Invoke();
}
