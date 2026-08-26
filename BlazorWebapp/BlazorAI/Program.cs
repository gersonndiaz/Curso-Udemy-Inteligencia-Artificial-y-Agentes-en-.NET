using BlazorAI.Components;
using BlazorAI.Domain.Context;
using BlazorAI.Services;
using BlazorAI.Services.Chatbots;
using BlazorAI.Tools;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.AI;
using OllamaSharp;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddDbContextFactory<AppDbContext>(options =>
    options.UseSqlite("Data Source=mydb.db"));

builder.Services.AddScoped<IPersonService, PersonService>();
builder.Services.AddScoped<IChatbot, Chatbot>();

builder.Services.AddTransient<IWeatherService, WeatherService>();
builder.Services.AddTransient<EvaluateConditions>();
builder.Services.AddTransient<EmailService>();
builder.Services.AddHttpClient();

builder.Services.AddTransient<IChatClientFactory, ChatClientFactory>();

// Movido a ChatClientFactory.cs
// var provider = "ollama";
// var model = "qwen3.5:9b";

// builder.Services.AddSingleton<IChatClient>(sp =>
// {
//     var configuration = sp.GetRequiredService<IConfiguration>();
//     var ollamaUrl = configuration["AI:OllamaUrl"] ?? "http://localhost:11434";
//     var keyOpenAI = configuration.GetValue<string>("OPENAI_KEY");
//     // var provider = configuration["AI:Provider"]?.Trim().ToLowerInvariant() ?? "ollama";
//     // var model = configuration["AI:Model"] ?? "qwen3.5:9b";

//     var cliente = provider switch
//     {
//         "ollama" => new OllamaApiClient(new Uri(ollamaUrl), model),
//         "openai" => new OpenAI.Chat.ChatClient(model, keyOpenAI).AsIChatClient(),
//         // "claude" => new AnthropicClient()
//         // {
//         //     ApiKey = llaveAnthropic
//         // }.AsIChatClient().AsBuilder().ConfigureOptions(c => c.ModelId = modelo ?? "claude-haiku-4-5").Build(),
//         _ => throw new ArgumentException($"Proveedor desconocido: {provider}")
//     };

//     return cliente.AsBuilder()
//     .UseFunctionInvocation(null, c =>
//     {
//         c.IncludeDetailedErrors = true;
//     })
//     .Build(sp);
// });

builder.Services.AddTransient<ChatOptions>(sp => new ChatOptions
{
    Tools = [.. Tools.GetTools(sp)],
    // ModelId = model,
    Temperature = 0.7f,
    MaxOutputTokens = 2000
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
