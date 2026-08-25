using System.Net.Http.Json;
using System.Text.Json.Serialization;

namespace Chatbot003.Services;

public class WeatherService(HttpClient httpClient) : IWeatherService
{
    public async Task<string> GetWeatherAsync(string location)
    {
        // // Simulación de una llamada a un servicio de clima
        // return location.ToLower().ToLowerInvariant() switch
        // {
        //     "londres" => "El clima en Londres es nublado con 15°C.",
        //     "paris" => "El clima en París es lluvioso con 18°C.",
        //     "copiapó" => "El clima en Copiapo es soleado con 22°C.",
        //     _ => $"No se tiene información del clima para {location}."
        // };

        string apiKey = Environment.GetEnvironmentVariable("CLIMA_API_KEY")!;
        string locationURL = Uri.EscapeDataString(location);
        string url = $"https://api.weatherapi.com/v1/current.json?key={apiKey}&q={locationURL}&aqi=no&lang=es";
        var wheatherResponse = await httpClient.GetFromJsonAsync<WeatherResponse>(url);
        return wheatherResponse!.Current.Condition.Text;
    }

    public class WeatherResponse
    {
        [JsonPropertyName("current")]
        public Current Current { get; set; } = default!;
    }

    public class Current
    {
        [JsonPropertyName("condition")]
        public Condition Condition { get; set; } = default!;
    }

    public class Condition
    {
        [JsonPropertyName("text")]
        public string Text { get; set; } = default!;
    }
}
