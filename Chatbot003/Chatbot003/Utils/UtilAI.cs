using System.Text;
using Microsoft.Extensions.AI;

namespace Chatbot003.Utils;

public class UtilAI
{
    internal static async Task GetChatbotResponseAsync(IChatClient chatClient)
    {
        // string systemPrompt = """
        //             Eres un asistente de inteligencia artificial llamado "Agente Ollama". Tu tarea es ayudar a los usuarios a responder preguntas y proporcionar información útil.
        //             Debes responder en Español
        //             Las respuestas deben ser claras, concisas y fáciles de entender. Evita dar respuestas vagas o ambiguas.
        //             Las respuestas deben ser en texto plano, no usar formato markdown ni HTML.
        //             Contesta como un experto en .net
        //         """ ;

        string systemPrompt = """
                    Eres un asistente de inteligencia artificial llamado "Agente Ollama". Tu tarea es ayudar a los usuarios a responder preguntas y proporcionar información útil.
                    Debes responder en Español
                    Las respuestas deben ser claras, concisas y fáciles de entender. Evita dar respuestas vagas o ambiguas.
                    Las respuestas deben ser en texto plano, no usar formato markdown ni HTML.
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
            
            while(true)
            {
                var updates = new List<ChatResponseUpdate>();

                await foreach (var responseUpdate in chatClient.GetStreamingResponseAsync(history))
                {
                    updates.Add(responseUpdate);
                    
                    foreach(var content in responseUpdate.Contents)
                    {
                        if (content is TextContent textContent)
                        {
                            Console.Write(textContent.Text);
                            sb.Append(textContent.Text);
                        }
                    }
                }

                var response = updates.ToChatResponse();
                history.AddMessages(response);

                var approvalRequest = response.Messages.SelectMany(m => m.Contents).OfType<ToolApprovalRequestContent>().FirstOrDefault();

                if (approvalRequest is not null)
                {
                    Console.WriteLine();
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine($"El modelo solicita usar la herramienta");

                    if (approvalRequest.ToolCall is FunctionCallContent functionCall)
                    {
                        Console.WriteLine($"Nombre de la función: {ConvertirNombreDeFuncion(functionCall.Name)}");

                        if (functionCall.Arguments is not null)
                        {
                            foreach (var arg in functionCall.Arguments)
                            {
                                Console.WriteLine($"Argumento: {arg.Key} = {arg.Value}");
                            }
                        }
                    }
                    
                    Console.ResetColor();

                    Console.Write("¿Deseas aprobar el uso de la herramienta? (s/n): ");
                    string approvalInput = Console.ReadLine() ?? string.Empty;
                    bool isApproved = approvalInput.Equals("s", StringComparison.OrdinalIgnoreCase);
                    ToolApprovalResponseContent approvalResponse = approvalRequest.CreateResponse(isApproved);

                    if (approvalInput.Equals("s", StringComparison.OrdinalIgnoreCase))
                    {
                        Console.WriteLine("Uso de la herramienta aprobado.");
                    }
                    else
                    {
                        Console.WriteLine("Uso de la herramienta denegado.");
                    }

                    history.Add(new ChatMessage(ChatRole.User, [approvalResponse]));

                    Console.WriteLine();
                    Console.WriteLine($"Chatbot: ");
                    continue; // Continuar con la siguiente iteración del bucle para obtener la respuesta del modelo después de la aprobación
                }
                // else
                // {
                //     break; // No hay solicitud de aprobación, salir del bucle
                // }

                Console.WriteLine();
                Console.WriteLine();
                break;
            }

            // // se usaba antes de usar tools con aprovacion de usuario
            // await foreach (var fragment in chatClient.GetStreamingResponseAsync(history))
            // {
            //     Console.Write(fragment);
            //     sb.Append(fragment);
            // }

            // Console.WriteLine();
            // string responseText = sb.ToString();
            // Console.WriteLine();
            // Console.WriteLine($"Longitud: {responseText.Length} caracteres");
            // Console.WriteLine();
            // Console.WriteLine();

            // history.Add(new ChatMessage(ChatRole.Assistant, responseText));
        }
    }

    private static string ConvertirNombreDeFuncion(string nombre)
    {
        return nombre switch
        {
            "SendEmailAsync" => "Enviar correo",
            _ => nombre
        };
    }
}
