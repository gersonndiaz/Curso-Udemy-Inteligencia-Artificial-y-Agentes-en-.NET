
using System.Text;
using Chatbot001;
using Microsoft.Extensions.AI;
using OllamaSharp;
// using OpenAI.Chat;

// var model = "gpt-5.6-luna";
// var client = new ChatClient(model, Constantes.ApiKeyOpenAI);

// Console.WriteLine("Chatbot001 - OpenAI ChatGPT");
// Console.WriteLine("Haz una pregunta");
// string? question = Console.ReadLine();

// if (string.IsNullOrEmpty(question))
// {
//     Console.WriteLine("No se ha ingresado ninguna pregunta.");
//     return;
// }

// var answer = await client.CompleteChatAsync(question);

// Console.WriteLine($"Respuesta: {answer}");
// Console.WriteLine("Presiona cualquier tecla para salir...");
// Console.ReadKey();

var model = "qwen3.5:9b";
var ollamaClient = new OllamaApiClient(new Uri("http://localhost:11434"), model);

Console.WriteLine($"Chatbot001 - Ollama API (Modelo: {model})");

// Console.WriteLine("Haz una pregunta");
// string? question = Console.ReadLine();

// if (string.IsNullOrEmpty(question))
// {
//     Console.WriteLine("No se ha ingresado ninguna pregunta.");
//     return;
// }

// var answer = await client.GetResponseAsync(question);//.CompleteChatAsync(question);
// Console.WriteLine($"Respuesta: {answer}");

Chat chat = new Chat(ollamaClient);
chat.Messages.Add(new OllamaSharp.Models.Chat.Message { 
        Role = OllamaSharp.Models.Chat.ChatRole.System, Content = """
            Eres un asistente de inteligencia artificial llamado "Chatbot001". Tu tarea es ayudar a los usuarios a responder preguntas y proporcionar información útil.
            Debes responder en Español
            Las respuestas deben ser claras, concisas y fáciles de entender. Evita dar respuestas vagas o ambiguas.
            Las respuestas deben ser en texto plano, no usar formato markdown ni HTML.
            Contesta como un experto en .net
        """ 
    });
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

    Console.WriteLine();
    // var response = chat.SendAsAsync(OllamaSharp.Models.Chat.ChatRole.User, userInput);
    // Console.WriteLine($"Chatbot: {response.FirstOrDefaultAsync().Result}");

    Console.Write("Chatbot: ");
    await foreach (var response in chat.SendAsync(userInput))
    {
        Console.Write(response);
        sb.Append(response);
    }

    Console.WriteLine();
    string responseText = sb.ToString();
    Console.WriteLine();
    Console.WriteLine($"Longitud: {responseText.Length} caracteres");
    Console.WriteLine();
}

Console.WriteLine("Presiona cualquier tecla para salir...");
Console.ReadKey();