using Microsoft.Extensions.VectorData;

namespace BlazorAI.RAG.Models;

public class FragmentDocumentVector
{
    [VectorStoreKey]
    public Guid Id { get; set; }
    [VectorStoreData(IsIndexed = true)]
    public string DocumentTitle { get; set; }
    [VectorStoreData(IsFullTextIndexed = true)]
    public string Text { get; set; }

    [VectorStoreVector(dimensions: 1536, DistanceFunction = DistanceFunction.CosineSimilarity)]
    public ReadOnlyMemory<float> Embedding { get; set; }
}
