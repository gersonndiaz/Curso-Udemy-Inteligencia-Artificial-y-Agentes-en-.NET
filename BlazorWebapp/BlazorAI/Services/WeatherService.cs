using System.Net.Http.Json;
using System.Text.Json.Serialization;

namespace BlazorAI.Services;

public class WeatherService(HttpClient httpClient, IConfiguration configuration) : IWeatherService
{
    public async Task<string> GetWeatherAsync(string location)
    {
        string apiKey = configuration.GetValue<string>("CLIMA_API_KEY")!;
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
