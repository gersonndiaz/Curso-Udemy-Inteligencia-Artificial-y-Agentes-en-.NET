using Chatbot003;
using Chatbot003.Chatbots;
using Chatbot003.Utils;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

// Console.WriteLine("Seleccione un chatbot:");
// Console.WriteLine("1. Ollama (Local)");
// Console.WriteLine("2. OpenAI (ChatGPT)");
// Console.WriteLine("3. Claude (Anthropic)");

// Console.Write("Opción: ");

// string opcion = Console.ReadLine() ?? "";

// string chatbot = opcion switch
// {
//     "1" => "Ollama",
//     "2" => "OpenAI",
//     "3" => "Claude",
//     _ => "Ollama"
// };

// Console.WriteLine($"Usando chatbot: {chatbot}");

// switch (chatbot)
// {
//     case "Ollama":
//         await ChatbotOllama.RunAsync();
//         break;
//     case "OpenAI":
//         Console.WriteLine("OpenAI no implementado aún");//await ChatbotOpenAI.RunAsync();
//         break;
//     case "Claude":
//         Console.WriteLine("Claude no implementado aún");//await ChatbotClaude.RunAsync();
//         break;
//     default:
//         Console.WriteLine("Chatbot no válido.");
//         break;
// }

InitUtils.LoadEnvironmentVariables();

var proveedor = args.Length > 0 ? args[0].ToLowerInvariant() : "ollama";
var modeloPorDefecto = proveedor == "ollama" ? "qwen3.5:9b" : "otro";
var modelo = args.Length > 1 ? args[1] : modeloPorDefecto;

var builder = Host.CreateApplicationBuilder(args);
Startup.ConfigureServices(builder, proveedor, modelo);
var host = builder.Build();

var chatClient = host.Services.GetRequiredService<IChatClient>();
await UtilAI.GetChatbotResponseAsync(chatClient);

Console.WriteLine("Presiona cualquier tecla para salir...");
Console.ReadKey();
