using Chatbot002.Chatbots;

Console.WriteLine("Seleccione un chatbot:");
Console.WriteLine("1. Ollama (Local)");
Console.WriteLine("2. OpenAI (ChatGPT)");
Console.WriteLine("3. Claude (Anthropic)");

Console.Write("Opción: ");

string opcion = Console.ReadLine() ?? "";

string chatbot = opcion switch
{
    "1" => "Ollama",
    "2" => "OpenAI",
    "3" => "Claude",
    _ => "Ollama"
};

Console.WriteLine($"Usando chatbot: {chatbot}");

switch (chatbot)
{
    case "Ollama":
        await ChatbotOllama.RunAsync();
        break;
    case "OpenAI":
        Console.WriteLine("OpenAI no implementado aún");//await ChatbotOpenAI.RunAsync();
        break;
    case "Claude":
        Console.WriteLine("Claude no implementado aún");//await ChatbotClaude.RunAsync();
        break;
    default:
        Console.WriteLine("Chatbot no válido.");
        break;
}

Console.WriteLine("Presiona cualquier tecla para salir...");
Console.ReadKey();
