﻿using Newtonsoft.Json;

namespace WeatherApp.Models
{

    // Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse);
    public class City
    {
        public int id { get; set; }
        public string? name { get; set; }
        public Coord? coord { get; set; }
        public string? country { get; set; }
        public int population { get; set; }
        public int timezone { get; set; }
        public int sunrise { get; set; }
        public int sunset { get; set; }
    }

    public class Clouds
    {
        public int all { get; set; }
    }

    public class Coord
    {
        public double lat { get; set; }
        public double lon { get; set; }
    }

    public class ForecastItem
    {
        public int dt { get; set; }
        public string dateTime => UtcTimeLibrary.UtcTimeStamp.ConvertToUtc(dt);
        // public string dateTime => DateTimeOffset.FromUnixTimeSeconds(dt).UtcDateTime.ToString("HH:mm");
        public Main? main { get; set; }
        public List<Weather>? weather { get; set; }
        public Clouds? clouds { get; set; }
        public Wind? wind { get; set; }
        public int visibility { get; set; }
        public double pop { get; set; }
        public Sys? sys { get; set; }
        public string? dt_txt { get; set; }
        public Rain? rain { get; set; }
        public string? DayOfWeek => DateTime.TryParse(dt_txt, out var date) ? date.ToString("dddd") : null;

    }

    public class Main
    {
        public double temp { get; set; }
        public double temperature => Math.Round(temp);
        public double feels_like { get; set; }
        public string FeelsLike => Math.Round(feels_like).ToString("0");
        public double temp_min { get; set; }
        public double temp_max { get; set; }
        public int pressure { get; set; }
        public int sea_level { get; set; }
        public int grnd_level { get; set; }
        public int humidity { get; set; }
        public double temp_kf { get; set; }
    }

    public class Rain
    {
        [JsonProperty("3h")]
        public double _3h { get; set; }
    }

    public class Root
    {
        public string? cod { get; set; }
        public int message { get; set; }
        public int cnt { get; set; }
        public List<ForecastItem>? list { get; set; }
        public City? city { get; set; }
    }

    public class Sys
    {
        public string? pod { get; set; }
    }

    public class Weather
    {
        public int id { get; set; }
        public string? main { get; set; }
        public string? description { get; set; }
        public string? icon { get; set; }
        public string customIcon => string.Format("icon_{0}.png", icon);
    }

    public class Wind
    {
        public double speed { get; set; }
        public int deg { get; set; }
        public double gust { get; set; }
    }


}
