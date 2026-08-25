using Chatbot003.Services;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OllamaSharp;

namespace Chatbot003;

public class Startup
{
    public static void ConfigureServices(HostApplicationBuilder builder, string proveedor, string? modelo)
    {
        string llaveOpenAI = Environment.GetEnvironmentVariable("OPENAI_LLAVE")!;
        string llaveAnthropic = Environment.GetEnvironmentVariable("ANTHROPIC_LLAVE")!;

        builder.Services.AddTransient<IWeatherService, WeatherService>();
        builder.Services.AddTransient<EvaluateConditions>();
        builder.Services.AddTransient<EmailService>();
        builder.Logging.AddFilter("System.Net.Http.HttpClient", LogLevel.None); // Evita que se muestren los logs de HttpClient en la consola
        builder.Services.AddHttpClient();

        builder.Services.AddSingleton<IChatClient>(sp =>
        {
            var cliente = proveedor switch
            {
                "ollama" => new OllamaApiClient(new Uri("http://localhost:11434"), modelo ?? "qwen3.5:9b"),
                "openai" => new OpenAI.Chat.ChatClient(modelo ?? "gpt-5.4-nano", llaveOpenAI).AsIChatClient(),
                // "claude" => new AnthropicClient()
                // {
                //     ApiKey = llaveAnthropic
                // }.AsIChatClient().AsBuilder().ConfigureOptions(c => c.ModelId = modelo ?? "claude-haiku-4-5").Build(),
                _ => throw new ArgumentException($"Proveedor desconocido: {proveedor}")
            };

            return cliente.AsBuilder()
            .ConfigureOptions(o =>
            {
                o.MaxOutputTokens = 2000;
                o.Temperature = 0.7f;
                o.Tools =[.. Tools.Tools.GetTools(sp)];
            })
            .UseFunctionInvocation(null, c =>
            {
                c.IncludeDetailedErrors = true;
            })
            .Use(async (mensajes, opciones, next, cancellationToken) =>
            {
            //    Console.WriteLine();
            //    Console.ForegroundColor = ConsoleColor.Green;
            //    Console.WriteLine("Antes de llamar al modelo...");
            //    Console.ResetColor();

               await next(mensajes, opciones, cancellationToken);

            //    Console.WriteLine();
            //    Console.ForegroundColor = ConsoleColor.Green;
            //    Console.WriteLine("Después de llamar al modelo...");
            //    Console.ResetColor();

            })
            .Build(sp);
            ;
        });
    }
}
