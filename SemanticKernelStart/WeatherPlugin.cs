using System.ComponentModel;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.SemanticKernel;

namespace SemanticKernelStart;

public class WeatherPlugin
{
    private readonly HttpClient _httpClient;
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public WeatherPlugin(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    [KernelFunction("current_weather")]
    [Description("Get the current weather for a city name using the Open-Meteo API.")]
    public async Task<string> GetCurrentWeatherAsync(
        [Description("City to look up, e.g. London or Seattle.")]
        string city,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(city))
        {
            return "Please provide a city name.";
        }

        var location = await ResolveLocationAsync(city, cancellationToken);
        if (location is null)
        {
            return $"I could not find a location for '{city}'.";
        }

        var weather = await GetWeatherAsync(location, cancellationToken);
        if (weather is null)
        {
            return $"I couldn't fetch weather data for {location.Name}.";
        }

        return BuildWeatherSummary(location, weather);
    }

    private async Task<Location?> ResolveLocationAsync(string city, CancellationToken cancellationToken)
    {
        var geocodeUrl =
            $"https://geocoding-api.open-meteo.com/v1/search?name={Uri.EscapeDataString(city)}&count=1&language=en&format=json";

        using var response = await _httpClient.GetAsync(geocodeUrl, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            return null;
        }

        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        var payload = await JsonSerializer.DeserializeAsync<GeocodingResponse>(stream, JsonOptions, cancellationToken);
        return payload?.Results?.FirstOrDefault();
    }

    private async Task<CurrentWeather?> GetWeatherAsync(Location location, CancellationToken cancellationToken)
    {
        var forecastUrl =
            $"https://api.open-meteo.com/v1/forecast?latitude={location.Latitude}&longitude={location.Longitude}&current_weather=true";

        using var response = await _httpClient.GetAsync(forecastUrl, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            return null;
        }

        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        var payload = await JsonSerializer.DeserializeAsync<ForecastResponse>(stream, JsonOptions, cancellationToken);
        return payload?.CurrentWeather;
    }

    private static string BuildWeatherSummary(Location location, CurrentWeather weather)
    {
        var description = WeatherCodeDescriptions.GetValueOrDefault(weather.WeatherCode, "Current conditions");
        return
            $"{description} in {location.Name} ({location.Latitude:F2}, {location.Longitude:F2}): " +
            $"{weather.Temperature:F1}°C, wind {weather.WindSpeed:F1} km/h at {weather.Time:u}.";
    }

    private sealed class GeocodingResponse
    {
        [JsonPropertyName("results")]
        public List<Location>? Results { get; set; }
    }

    private sealed class Location
    {
        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("latitude")]
        public double Latitude { get; set; }

        [JsonPropertyName("longitude")]
        public double Longitude { get; set; }
    }

    private sealed class ForecastResponse
    {
        [JsonPropertyName("current_weather")]
        public CurrentWeather? CurrentWeather { get; set; }
    }

    private sealed class CurrentWeather
    {
        [JsonPropertyName("temperature")]
        public double Temperature { get; set; }

        [JsonPropertyName("windspeed")]
        public double WindSpeed { get; set; }

        [JsonPropertyName("weathercode")]
        public int WeatherCode { get; set; }

        [JsonPropertyName("time")]
        public DateTime Time { get; set; }
    }

    private static readonly Dictionary<int, string> WeatherCodeDescriptions = new()
    {
        { 0, "Clear sky" },
        { 1, "Mainly clear" },
        { 2, "Partly cloudy" },
        { 3, "Overcast" },
        { 45, "Foggy" },
        { 48, "Depositing rime fog" },
        { 51, "Light drizzle" },
        { 53, "Moderate drizzle" },
        { 55, "Dense drizzle" },
        { 56, "Light freezing drizzle" },
        { 57, "Dense freezing drizzle" },
        { 61, "Slight rain" },
        { 63, "Moderate rain" },
        { 65, "Heavy rain" },
        { 66, "Light freezing rain" },
        { 67, "Heavy freezing rain" },
        { 71, "Slight snow fall" },
        { 73, "Moderate snow fall" },
        { 75, "Heavy snow fall" },
        { 77, "Snow grains" },
        { 80, "Slight rain showers" },
        { 81, "Moderate rain showers" },
        { 82, "Violent rain showers" },
        { 85, "Slight snow showers" },
        { 86, "Heavy snow showers" },
        { 95, "Thunderstorm" },
        { 96, "Thunderstorm with slight hail" },
        { 99, "Thunderstorm with heavy hail" }
    };
}
