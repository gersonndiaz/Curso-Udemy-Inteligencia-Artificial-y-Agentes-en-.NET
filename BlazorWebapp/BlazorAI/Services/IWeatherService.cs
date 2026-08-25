namespace BlazorAI.Services;

interface IWeatherService
{
    Task<string> GetWeatherAsync(string location);
}
