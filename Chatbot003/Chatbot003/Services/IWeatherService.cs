namespace Chatbot003.Services;

interface IWeatherService
{
    Task<string> GetWeatherAsync(string location);
}
