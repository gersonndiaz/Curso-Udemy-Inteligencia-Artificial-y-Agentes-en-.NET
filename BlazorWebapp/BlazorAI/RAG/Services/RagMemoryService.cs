using BlazorAI.RAG.Models;
using CommunityToolkit.VectorData.InMemory;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.VectorData;

namespace BlazorAI.RAG.Services;

public class RagMemoryService : IRagService
{
    private readonly DocumentsInMemoryService documentsInMemoryService;
    private readonly IEmbeddingGenerator<string, Embedding<float>> embeddingGenerator;
    private readonly VectorStoreCollection<Guid, FragmentDocumentVector> collection;
    private bool isInitialized;
    // private readonly InMemoryVectorStore vectorStore;

    public RagMemoryService(DocumentsInMemoryService documentsInMemoryService
                            , IEmbeddingGenerator<string, Embedding<float>> embeddingGenerator
                            , InMemoryVectorStore vectorStore
    )
    {
        this.documentsInMemoryService = documentsInMemoryService;
        this.embeddingGenerator = embeddingGenerator;
        // this.vectorStore = vectorStore;
        collection = vectorStore.GetCollection<Guid, FragmentDocumentVector>("documentos");  // Esto es como la tabla de la DB en memoria
    }

    public async Task Init(CancellationToken cancellationToken = default)
    {
        if (isInitialized)
        {
            return;
        }

        await collection.EnsureCollectionExistsAsync(cancellationToken);

        var documents = documentsInMemoryService.GetDocuments();

        foreach(var document in documents)
        {
            var fragments = document.Content.Split("\n", StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList();

            foreach(var fragment in fragments)
            {
                var vector = await embeddingGenerator.GenerateVectorAsync(fragment, cancellationToken: cancellationToken);

                var fVector = new FragmentDocumentVector
                {
                    Id = Guid.NewGuid(),
                    DocumentTitle = document.Title,
                    Text = fragment,
                    Embedding = vector
                };

                await collection.UpsertAsync(fVector, cancellationToken);
            }
        }

        isInitialized = true;
    }

    public async Task<List<string>> SearchContext(string question, int top = 3, float minScore = 0.6f, CancellationToken cancellationToken = default)
    {
        await Init(cancellationToken);

        var questionEmbedding = await embeddingGenerator.GenerateVectorAsync(question, cancellationToken: cancellationToken);
        var results = new List<string>();

        await foreach(var result in collection.SearchAsync(questionEmbedding, top: top, cancellationToken: cancellationToken))
        {
            // Si no cumple con el score minimo, continua sin considerar el resultado obtenido
            if (result.Score < minScore)
            {
                continue;
            }

            results.Add($"""
                Documento: {result.Record.DocumentTitle}
                Contenido: {result.Record.Text}
            """);
        }

        return results;
    }
}
