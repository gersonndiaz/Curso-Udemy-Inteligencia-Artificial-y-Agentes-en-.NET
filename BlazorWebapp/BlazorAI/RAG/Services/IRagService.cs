namespace BlazorAI.RAG.Services;

public interface IRagService
{
    Task Init(CancellationToken cancellationToken = default);
    Task<List<string>> SearchContext(string question, int top = 3, float minScore = 0.6f, CancellationToken cancellationToken = default);
}
