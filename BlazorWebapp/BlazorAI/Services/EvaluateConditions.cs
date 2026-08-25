namespace BlazorAI.Services;

public class EvaluateConditions
{
    public string Evaluate(string condition)
    {
        // return condition.ToLower() switch
        // {
        //     "soleado" => "El clima es soleado, ¡disfruta del día!",
        //     "nublado" => "El clima está nublado, podrías necesitar un abrigo ligero.",
        //     "lluvioso" => "Está lloviendo, no olvides tu paraguas.",
        //     _ => $"No se tiene información sobre el clima: {condition}."
        // };

        string condicionDelClima = condition.ToLower();

        // Lluvia / Llovizna / Precipitaciones
        if (condicionDelClima.Contains("lluvia") ||
            condicionDelClima.Contains("llovizna") ||
            condicionDelClima.Contains("precipitaciones"))
            return "No es un buen momento para actividades al aire libre";

        // Tormentas
        if (condicionDelClima.Contains("tormenta") ||
            condicionDelClima.Contains("tormentosos"))
            return "Evita salir, condiciones climáticas peligrosas";

        // Nieve / Nevadas / Ventisca
        if (condicionDelClima.Contains("nieve") ||
            condicionDelClima.Contains("nevadas") ||
            condicionDelClima.Contains("ventisca"))
            return "Condiciones frías y posiblemente peligrosas, sal solo si es necesario";

        // Neblina / Niebla
        if (condicionDelClima.Contains("neblina") ||
            condicionDelClima.Contains("niebla"))
            return "Precaución al salir, la visibilidad puede estar reducida";

        // Soleado
        if (condicionDelClima.Contains("soleado"))
            return "Excelente clima para salir";

        // Nublado
        if (condicionDelClima.Contains("nublado"))
            return "Puedes salir, pero no es el clima ideal";

        return "Condiciones normales";
    }
}
