using Chatbot003.Utils;
using Microsoft.Extensions.AI;
using OllamaSharp;

namespace Chatbot003.Chatbots;

public class ChatbotOllama
{
    internal static async Task RunAsync()
    {
        var model = "qwen3.5:9b";
        Console.WriteLine($"Chatbot002 - Ollama API (Modelo: {model})");

        IChatClient chatClient = new OllamaApiClient(new Uri("http://localhost:11434"), model);
        await UtilAI.GetChatbotResponseAsync(chatClient);

        // IChatClient chatClient = new OllamaApiClient(new Uri("http://localhost:11434"), model);
        // chatClient.AsBuilder().ConfigureOptions(o =>
        // {
        //     o.MaxOutputTokens = 100;
        //     o.Temperature = 0.7f;
        // })
        // .Build();
        // await _ChatbotUtil.GetChatbotResponseAsync(chatClient);
    }
}
