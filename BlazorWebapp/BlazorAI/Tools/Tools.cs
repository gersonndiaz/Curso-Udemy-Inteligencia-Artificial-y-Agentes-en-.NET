using BlazorAI.Services;
using Microsoft.Extensions.AI;

namespace BlazorAI.Tools;

public static class Tools
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

        var personService = sp.GetRequiredService<IPersonService>();
        yield return AIFunctionFactory.Create(personService.GetAll);
    }
}
