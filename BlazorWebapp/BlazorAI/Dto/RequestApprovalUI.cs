using Microsoft.Extensions.AI;

namespace BlazorAI.Dto;

public class RequestApprovalUI
{
    public required ToolApprovalRequestContent ToolApprovalRequest { get; set; }
    public required string ToolName { get; set; }
    public Dictionary<string, object?> Arguments { get; set; } = [];
}
