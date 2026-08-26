using OllamaSharp;

namespace BlazorAI.Helpers;

public static class ModelsAI
{
    private static readonly Dictionary<string, string> Models = new(StringComparer.OrdinalIgnoreCase)
    {
        ["gpt-5.4-nano"] = "openai",
        ["qwen3.5:9b"] = "ollama"
    };

    public static async Task LoadOllamaModelsAsync(
        string ollamaUrl = "http://localhost:11434")
    {
        var ollamaModels = await GetModelsOllamaAsync(ollamaUrl);
        if (ollamaModels is null)
        {
            return;
        }

        foreach (var model in ollamaModels)
        {
            Models[model] = "ollama";
        }
    }

    private static async Task<IEnumerable<string>?> GetModelsOllamaAsync(string ollamaUrl)
    {
        var ollamaClient = new OllamaApiClient(
            new Uri(ollamaUrl)
        );

        var ollamaModels = await ollamaClient.ListLocalModelsAsync();
        return ollamaModels?.Select(model => model.Name);
    }

    public static string GetProvider(string model)
    {
        if (Models.TryGetValue(model, out var provider))
        {
            return provider;
        }

        throw new ArgumentException($"Modelo no soportado: {model}");
    }

    public static IEnumerable<string> GetModelsAvailables() => Models.Keys;
    public static string GetModelDefault => "qwen3.5:9b";
}
