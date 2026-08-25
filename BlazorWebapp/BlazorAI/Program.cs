using BlazorAI.Components;
using BlazorAI.Services.Chatbots;
using Microsoft.Extensions.AI;
using OllamaSharp;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddScoped<IChatbot, Chatbot>();

builder.Services.AddSingleton<IChatClient>(sp =>
{
    var configuration = sp.GetRequiredService<IConfiguration>();
    var provider = configuration["AI:Provider"]?.Trim().ToLowerInvariant() ?? "ollama";
    var model = configuration["AI:Model"] ?? "qwen3.5:9b";
    var ollamaUrl = configuration["AI:OllamaUrl"] ?? "http://localhost:11434";
    var keyOpenAI = configuration.GetValue<string>("OPENAI_KEY");

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
    .ConfigureOptions(o =>
    {
        o.MaxOutputTokens = 2000;
        o.Temperature = 0.7f;
        //o.Tools =[.. Tools.Tools.GetTools(sp)];
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
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
}
app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
