using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace AliceNeural.Models
{

    // Root myDeserializedClass = JsonSerializer.Deserialize<Root>(myJsonResponse);
    public class Current
    {
        [JsonPropertyName("time")]
        public int? Time { get; set; }

        [JsonPropertyName("interval")]
        public int? Interval { get; set; }

        [JsonPropertyName("temperature_2m")]
        public double? Temperature2m { get; set; }

        [JsonPropertyName("weather_code")]
        public int? WeatherCode { get; set; }

        [JsonPropertyName("wind_speed_10m")]
        public double? WindSpeed10m { get; set; }

        [JsonPropertyName("wind_direction_10m")]
        public int? WindDirection10m { get; set; }
    }

    public class CurrentUnits
    {
        [JsonPropertyName("time")]
        public string? Time { get; set; }

        [JsonPropertyName("interval")]
        public string? Interval { get; set; }

        [JsonPropertyName("temperature_2m")]
        public string? Temperature2m { get; set; }

        [JsonPropertyName("weather_code")]
        public string? WeatherCode { get; set; }

        [JsonPropertyName("wind_speed_10m")]
        public string? WindSpeed10m { get; set; }

        [JsonPropertyName("wind_direction_10m")]
        public string? WindDirection10m { get; set; }
    }

    public class Daily
    {
        [JsonPropertyName("time")]
        public List<int?> Time { get; set; }
        public string IconPath { get; set; }

        [JsonPropertyName("weather_code")]
        public List<int?> WeatherCode { get; set; }

        [JsonPropertyName("temperature_2m_max")]
        public List<double?> Temperature2mMax { get; set; }

        [JsonPropertyName("temperature_2m_min")]
        public List<double> Temperature2mMin { get; set; }

        [JsonPropertyName("apparent_temperature_max")]
        public List<double> ApparentTemperatureMax { get; set; }

        [JsonPropertyName("apparent_temperature_min")]
        public List<double> ApparentTemperatureMin { get; set; }
    }

    public class DailyUnits
    {
        [JsonPropertyName("time")]
        public string? Time { get; set; }

        [JsonPropertyName("weather_code")]
        public string? WeatherCode { get; set; }

        [JsonPropertyName("temperature_2m_max")]
        public string? Temperature2mMax { get; set; }

        [JsonPropertyName("temperature_2m_min")]
        public string? Temperature2mMin { get; set; }

        [JsonPropertyName("apparent_temperature_max")]
        public string? ApparentTemperatureMax { get; set; }

        [JsonPropertyName("apparent_temperature_min")]
        public string? ApparentTemperatureMin { get; set; }
    }

    public class Hourly
    {
        [JsonPropertyName("time")]
        public List<int?> Time { get; set; }

        [JsonPropertyName("temperature_2m")]
        public List<double> Temperature2m { get; set; }

        [JsonPropertyName("relative_humidity_2m")]
        public List<int?>? RelativeHumidity2m { get; set; }

        [JsonPropertyName("dew_point_2m")]
        public List<double>? DewPoint2m { get; set; }

        [JsonPropertyName("apparent_temperature")]
        public List<double> ApparentTemperature { get; set; }

        [JsonPropertyName("precipitation_probability")]
        public List<int?> PrecipitationProbability { get; set; }

        [JsonPropertyName("precipitation")]
        public List<double> Precipitation { get; set; }

        [JsonPropertyName("rain")]
        public List<double>? Rain { get; set; }

        [JsonPropertyName("showers")]
        public List<double> Showers { get; set; }

        [JsonPropertyName("weather_code")]
        public List<int?> WeatherCode { get; set; }

        [JsonPropertyName("wind_speed_10m")]
        public List<double> WindSpeed10m { get; set; }

        [JsonPropertyName("wind_direction_10m")]
        public List<int?> WindDirection10m { get; set; }
    }

    public class HourlyUnits
    {
        [JsonPropertyName("time")]
        public string? Time { get; set; }

        [JsonPropertyName("temperature_2m")]
        public string? Temperature2m { get; set; }

        [JsonPropertyName("relative_humidity_2m")]
        public string? RelativeHumidity2m { get; set; }

        [JsonPropertyName("dew_point_2m")]
        public string? DewPoint2m { get; set; }

        [JsonPropertyName("apparent_temperature")]
        public string? ApparentTemperature { get; set; }

        [JsonPropertyName("precipitation_probability")]
        public string? PrecipitationProbability { get; set; }

        [JsonPropertyName("precipitation")]
        public string? Precipitation { get; set; }

        [JsonPropertyName("rain")]
        public string? Rain { get; set; }

        [JsonPropertyName("showers")]
        public string? Showers { get; set; }

        [JsonPropertyName("weather_code")]
        public string? WeatherCode { get; set; }

        [JsonPropertyName("wind_speed_10m")]
        public string? WindSpeed10m { get; set; }

        [JsonPropertyName("wind_direction_10m")]
        public string? WindDirection10m { get; set; }
    }

    public class OpenMeteoForecast
    {
        [JsonPropertyName("latitude")]
        public double? Latitude { get; set; }

        [JsonPropertyName("longitude")]
        public double? Longitude { get; set; }

        [JsonPropertyName("generationtime_ms")]
        public double? GenerationtimeMs { get; set; }

        [JsonPropertyName("utc_offset_seconds")]
        public int? UtcOffsetSeconds { get; set; }

        [JsonPropertyName("timezone")]
        public string? Timezone { get; set; }

        [JsonPropertyName("timezone_abbreviation")]
        public string? TimezoneAbbreviation { get; set; }

        [JsonPropertyName("elevation")]
        public double? Elevation { get; set; }

        [JsonPropertyName("current_units")]
        public CurrentUnits CurrentUnits { get; set; }

        [JsonPropertyName("current")]
        public Current Current { get; set; }

        [JsonPropertyName("hourly_units")]
        public HourlyUnits HourlyUnits { get; set; }

        [JsonPropertyName("hourly")]
        public Hourly Hourly { get; set; }

        [JsonPropertyName("daily_units")]
        public DailyUnits DailyUnits { get; set; }

        [JsonPropertyName("daily")]
        public Daily Daily { get; set; }
    }
}
