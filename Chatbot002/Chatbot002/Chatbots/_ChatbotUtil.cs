using System.Text;
using Microsoft.Extensions.AI;

namespace Chatbot002.Chatbots;

public class _ChatbotUtil
{
    internal static async Task GetChatbotResponseAsync(IChatClient chatClient)
    {
        string systemPrompt = """
                    Eres un asistente de inteligencia artificial llamado "Chatbot002". Tu tarea es ayudar a los usuarios a responder preguntas y proporcionar información útil.
                    Debes responder en Español
                    Las respuestas deben ser claras, concisas y fáciles de entender. Evita dar respuestas vagas o ambiguas.
                    Las respuestas deben ser en texto plano, no usar formato markdown ni HTML.
                    Contesta como un experto en .net
                """ ;
        
        var history = new List<ChatMessage>();
        history.Add(new ChatMessage(ChatRole.System, systemPrompt));
        while(true)
        {
            StringBuilder sb = new StringBuilder();
            Console.ForegroundColor = ConsoleColor.Blue;
            Console.Write("Tú: ");
            string userInput = Console.ReadLine() ?? string.Empty;
            Console.ResetColor();

            if (string.IsNullOrEmpty(userInput))
            {
                Console.WriteLine("No se ha ingresado ninguna pregunta.");
                break;
            }

            history.Add(new ChatMessage(ChatRole.User, userInput));

            Console.WriteLine();
            Console.Write("Chatbot: ");
            await foreach (var fragment in chatClient.GetStreamingResponseAsync(history))
            {
                Console.Write(fragment);
                sb.Append(fragment);
            }

            Console.WriteLine();
            string responseText = sb.ToString();
            Console.WriteLine();
            Console.WriteLine($"Longitud: {responseText.Length} caracteres");
            Console.WriteLine();
            Console.WriteLine();

            history.Add(new ChatMessage(ChatRole.Assistant, responseText));
        }
    }
}
