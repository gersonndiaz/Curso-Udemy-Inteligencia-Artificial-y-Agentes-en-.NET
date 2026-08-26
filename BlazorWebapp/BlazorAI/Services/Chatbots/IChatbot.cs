using BlazorAI.Dto;

namespace BlazorAI.Services.Chatbots;

public interface IChatbot
{
    List<MessageChatUI> Conversation { get; }
    bool IsProcessing { get; }
    RequestApprovalUI RequestApprovalPending { get; }

    event Action? OnChange;

    void CancelCurrentResponse();
    Task SendMessageAsync(string userText, CancellationToken cancellationToken = default);
    Task ResolveApprovalAsync(bool approved, CancellationToken cancellationToken = default);
}
