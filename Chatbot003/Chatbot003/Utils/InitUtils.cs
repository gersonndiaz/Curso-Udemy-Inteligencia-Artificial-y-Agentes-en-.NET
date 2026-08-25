namespace Chatbot003.Utils;

public class InitUtils
{
    internal static void LoadEnvironmentVariables()
    {
        foreach (var linea in File.ReadAllLines(".env"))
        {
            // LLAVE=VALOR
            var partes = linea.Split("=");
            if (partes.Length == 2)
            {
                Environment.SetEnvironmentVariable(partes[0], partes[1]);
            }
        }
    }
}
