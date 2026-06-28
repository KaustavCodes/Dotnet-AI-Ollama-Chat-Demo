using System.Net.Http.Json;
using System.Text.Json;

namespace VectorDataSearch;

public static class WeatherService
{
    private static readonly HttpClient _httpClient = new HttpClient();
    private static readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true
    };

    public static async Task<String> GetWeatherAndAirQualityAsync(string cityName)
    {
        Console.WriteLine($" [TOOL CALLED] Weather → {cityName}");
        var location = await GetCoordinatesAsync(cityName);
        
        var weatherTask = GetWeatherAsync(location.Latitude, location.Longitude);
        var airQualityTask = GetAirQualityAsync(location.Latitude, location.Longitude);

        await Task.WhenAll(weatherTask, airQualityTask);

        // return new WeatherResult
        // {
        //     Location = location,
        //     Weather = await weatherTask,
        //     AirQuality = await airQualityTask
        // };

        var weatherResult = await weatherTask;
        var airQuality = await airQualityTask; 

        List<string> weatherInfo = new List<string>
        {
            $"Location: {location.Name}, {location.Admin1}, {location.Country}",
            $"Coordinates: {location.Latitude}, {location.Longitude}",
            $"Current Weather: {weatherResult?.Current.Temperature}°C, {weatherResult?.Current.Condition}, Feels Like: {weatherResult?.Current.FeelsLike}°C, Humidity: {weatherResult?.Current.Humidity}%, Wind Speed: {weatherResult?.Current.WindSpeed} m/s"
        };

        return string.Join(Environment.NewLine, weatherInfo);
    }

    private static async Task<Location> GetCoordinatesAsync(string cityName)
    {
        var url = $"https://geocoding-api.open-meteo.com/v1/search?name={Uri.EscapeDataString(cityName)}&count=1&language=en&format=json";
        var response = await _httpClient.GetFromJsonAsync<GeocodingResponse>(url, _jsonOptions);

        if (response?.Results == null || response.Results.Count == 0)
            throw new Exception($"No location found for '{cityName}'");

        var first = response.Results[0];
        return new Location
        {
            Name = first.Name,
            Admin1 = first.Admin1,
            Country = first.Country,
            Latitude = first.Latitude,
            Longitude = first.Longitude
        };
    }

    private static async Task<WeatherData> GetWeatherAsync(double lat, double lon)
    {
        var url = $"https://api.open-meteo.com/v1/forecast?latitude={lat}&longitude={lon}" +
                  "&current=temperature_2m,relative_humidity_2m,apparent_temperature,weather_code,wind_speed_10m" +
                  "&daily=weather_code,temperature_2m_max,temperature_2m_min,sunrise,sunset" +
                  "&timezone=auto&forecast_days=1";

        var data = await _httpClient.GetFromJsonAsync<WeatherResponse>(url, _jsonOptions);
        return new WeatherData(data);
    }

    private static async Task<AirQualityData> GetAirQualityAsync(double lat, double lon)
    {
        var url = $"https://air-quality-api.open-meteo.com/v1/air-quality?latitude={lat}&longitude={lon}" +
                  "&current=pm10,pm2_5,european_aqi,us_aqi" +
                  "&timezone=auto";

        var data = await _httpClient.GetFromJsonAsync<AirQualityResponse>(url, _jsonOptions);
        return new AirQualityData(data);
    }
}