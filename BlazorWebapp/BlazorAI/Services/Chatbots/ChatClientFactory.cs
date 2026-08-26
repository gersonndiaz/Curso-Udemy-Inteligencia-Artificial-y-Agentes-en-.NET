using BlazorAI.Helpers;
using Microsoft.Extensions.AI;
using OllamaSharp;

namespace BlazorAI.Services.Chatbots;

public class ChatClientFactory(IConfiguration configuration, IServiceProvider sp) : IChatClientFactory
{
    public IChatClient Create(string model)
    {
        var ollamaUrl = configuration["AI:OllamaUrl"] ?? "http://localhost:11434";
        var keyOpenAI = configuration.GetValue<string>("OPENAI_KEY");
        var provider = ModelsAI.GetProvider(model);

        var cliente = provider switch
        {
            "ollama" => new OllamaApiClient(new Uri(ollamaUrl), model),
            "openai" => new OpenAI.Chat.ChatClient(model, keyOpenAI).AsIChatClient(),
            // "claude" => new AnthropicClient()
            // {
            //     ApiKey = llaveAnthropic
            // }.AsIChatClient().AsBuilder().ConfigureOptions(c => c.ModelId = modelo ?? "claude-haiku-4-5").Build(),
            _ => throw new ArgumentException($"Proveedor desconocido: {provider}")
        };

        return cliente.AsBuilder()
        .UseFunctionInvocation(null, c =>
        {
            c.IncludeDetailedErrors = true;
        })
        .Build(sp);
    }
}
