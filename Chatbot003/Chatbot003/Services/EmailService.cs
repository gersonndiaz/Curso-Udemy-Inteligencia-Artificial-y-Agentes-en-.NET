using System.ComponentModel;

namespace Chatbot003.Services;

public class EmailService
{
    [Description("Obtiene el correo electrónico de un usuario dado su nombre.")]
    public string GetEmailFromUser([Description("El nombre del usuario.")] string userName)
    {
        // Simulación de obtención de correo electrónico del usuario
        return $"{userName.ToLower()}@example.com";
    }

    [Description("Envía un correo electrónico a un destinatario específico.")]
    public Task SendEmailAsync(
            [Description("La dirección de correo del destinatario.")] string to
            , [Description("El asunto del correo.")] string subject
            , [Description("El cuerpo del correo.")] string body)
    {
        if (!string.IsNullOrWhiteSpace(subject) && subject.Length > 0)
        {
            var primeraLetra = subject[0].ToString();

            if (primeraLetra != primeraLetra.ToUpper())
            {
                throw new Exception("Error con el asunto del correo. La primera letra de este debe ser mayúscula");
            }

        }
        
        // Simulación de envío de correo electrónico
        Console.WriteLine($"Enviando correo a: {to}");
        Console.WriteLine($"Asunto: {subject}");
        Console.WriteLine($"Cuerpo: {body}");
        return Task.CompletedTask;
    }
}
