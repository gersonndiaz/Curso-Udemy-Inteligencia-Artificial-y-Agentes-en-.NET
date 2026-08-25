using Chatbot003.Services;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;

namespace Chatbot003.Tools;

internal static class Tools
{
    internal static IEnumerable<AITool> GetTools(this IServiceProvider sp)
    {
        var weatherService = sp.GetRequiredService<IWeatherService>();

        yield return AIFunctionFactory.Create(
            weatherService.GetWeatherAsync,
            new AIFunctionFactoryOptions
            {
                Name = "GetWeatherAsync",
                Description = "Obtiene información del clima para una ubicación específica.",
            }
        );

        var evaluateConditions = sp.GetRequiredService<EvaluateConditions>();

        yield return AIFunctionFactory.Create(
            evaluateConditions.Evaluate,
            new AIFunctionFactoryOptions
            {
                Name = "Evaluate",
                Description = "Evalúa las condiciones climáticas y proporciona recomendaciones.",
            }
        );

        var emailService = sp.GetRequiredService<EmailService>();
        yield return AIFunctionFactory.Create(emailService.GetEmailFromUser);

        var functionEmailSender = AIFunctionFactory.Create(emailService.SendEmailAsync);
        yield return new ApprovalRequiredAIFunction(functionEmailSender);

        // var tools = new List<AITool>
        // {
        //     new AITool(
        //         name: "WeatherTool",
        //         description: "Obtiene información del clima para una ubicación específica.",
        //         func: async (input) =>
        //         {
        //             string location = input.ToString() ?? string.Empty;
        //             return await weatherService.GetWeatherAsync(location);
        //         }
        //     )
        // };

        // return tools;
    }
}
