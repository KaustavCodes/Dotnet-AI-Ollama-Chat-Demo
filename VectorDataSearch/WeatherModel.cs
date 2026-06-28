namespace VectorDataSearch
{

    public class Location
    {
        public string Name { get; set; } = string.Empty;
        public string? Admin1 { get; set; }
        public string? Country { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
    }

    public class WeatherResult
    {
        public Location Location { get; set; } = new();
        public WeatherData Weather { get; set; } = new();
        public AirQualityData AirQuality { get; set; } = new();
    }

    public class WeatherData
    {
        public CurrentWeather Current { get; set; } = new();
        public List<DailyWeather> Daily { get; set; } = new();

        public WeatherData() { }
        public WeatherData(WeatherResponse? resp)
        {
            if (resp?.Current != null)
                Current = new CurrentWeather(resp.Current);

            if (resp?.Daily != null)
            {
                for (int i = 0; i < Math.Min(5, resp.Daily.Time.Count); i++)
                {
                    Daily.Add(new DailyWeather
                    {
                        Date = resp.Daily.Time[i],
                        WeatherCode = resp.Daily.WeatherCode.Count > i ? resp.Daily.WeatherCode[i] : 0,
                        TempMax = resp.Daily.Temperature2mMax.Count > i ? resp.Daily.Temperature2mMax[i] : 0,
                        TempMin = resp.Daily.Temperature2mMin.Count > i ? resp.Daily.Temperature2mMin[i] : 0
                    });
                }
            }
        }
    }

    public class CurrentWeather
    {
        public string Time { get; set; } = string.Empty;
        public double Temperature { get; set; }
        public double FeelsLike { get; set; }
        public int Humidity { get; set; }
        public double WindSpeed { get; set; }
        public int WeatherCode { get; set; }
        public string Condition => WeatherModelHelper.InterpretWeatherCode(WeatherCode);

        public CurrentWeather() { }
        public CurrentWeather(Current current)
        {
            Time = current.Time;
            Temperature = current.Temperature2m;
            FeelsLike = current.ApparentTemperature;
            Humidity = current.RelativeHumidity2m;
            WindSpeed = current.WindSpeed10m;
            WeatherCode = current.WeatherCode;
        }
    }

    public class DailyWeather
    {
        public string Date { get; set; } = string.Empty;
        public int WeatherCode { get; set; }
        public double TempMax { get; set; }
        public double TempMin { get; set; }
        public string Condition => WeatherModelHelper.InterpretWeatherCode(WeatherCode);
    }

    public class AirQualityData
    {
        public double? Pm25 { get; set; }
        public double? Pm10 { get; set; }
        public int? EuAqi { get; set; }
        public int? UsAqi { get; set; }
        public string EuDescription => EuAqi.HasValue ? WeatherModelHelper.GetAqiDescription(EuAqi.Value, true) : "N/A";
        public string UsDescription => UsAqi.HasValue ? WeatherModelHelper.GetAqiDescription(UsAqi.Value, false) : "N/A";

        public AirQualityData() { }
        public AirQualityData(AirQualityResponse? resp)
        {
            if (resp?.Current != null)
            {
                Pm25 = resp.Current.Pm2_5;
                Pm10 = resp.Current.Pm10;
                EuAqi = resp.Current.EuropeanAqi;
                UsAqi = resp.Current.UsAqi;
            }
        }
    }

    // ==================== Raw API Models ====================

    public class GeocodingResponse
    {
        public List<GeocodingResult> Results { get; set; } = new();
    }

    public class GeocodingResult
    {
        public string Name { get; set; } = string.Empty;
        public string? Admin1 { get; set; }
        public string? Country { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
    }

    public class WeatherResponse
    {
        public Current Current { get; set; } = new();
        public Daily Daily { get; set; } = new();
    }

    public class Current
    {
        public string Time { get; set; } = string.Empty;
        public double Temperature2m { get; set; }
        public double ApparentTemperature { get; set; }
        public int RelativeHumidity2m { get; set; }
        public double WindSpeed10m { get; set; }
        public int WeatherCode { get; set; }
    }

    public class Daily
    {
        public List<string> Time { get; set; } = new();
        public List<int> WeatherCode { get; set; } = new();
        public List<double> Temperature2mMax { get; set; } = new();
        public List<double> Temperature2mMin { get; set; } = new();
    }

    public class AirQualityResponse
    {
        public CurrentAirQuality Current { get; set; } = new();
    }

    public class CurrentAirQuality
    {
        public double? Pm2_5 { get; set; }
        public double? Pm10 { get; set; }
        public int? EuropeanAqi { get; set; }
        public int? UsAqi { get; set; }
    }

    // ==================== Helper Methods ====================

    public static class WeatherModelHelper
    {
        public static string InterpretWeatherCode(int code)
        {
            return code switch
            {
                0 => "Clear sky",
                1 or 2 => "Partly cloudy",
                3 => "Overcast",
                45 or 48 => "Fog",
                >= 51 and <= 55 => "Drizzle",
                >= 61 and <= 65 => "Rain",
                >= 71 and <= 75 => "Snow",
                95 => "Thunderstorm",
                _ => $"Code {code}"
            };
        }

        public static string GetAqiDescription(int aqi, bool isEu)
        {
            if (isEu)
                return aqi switch
                {
                    <= 20 => "Good",
                    <= 40 => "Fair",
                    <= 60 => "Moderate",
                    <= 80 => "Poor",
                    <= 100 => "Very Poor",
                    _ => "Extremely Poor"
                };
            else
                return aqi switch
                {
                    <= 50 => "Good",
                    <= 100 => "Moderate",
                    <= 150 => "Unhealthy for Sensitive Groups",
                    <= 200 => "Unhealthy",
                    <= 300 => "Very Unhealthy",
                    _ => "Hazardous"
                };
        }
    }
}
