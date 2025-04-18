using Newtonsoft.Json;
using WeatherApp.Models;

namespace WeatherApp.Services

//api.openweathermap.org/data/2.5/forecast? q = { city name }&appid={API key}
//https://api.openweathermap.org/data/2.5/forecast?lat=55.704660&lon=12.542107&units=metric&appid=1aa634bc0231b4af1904ccd51ed1604a
{
    internal static class ApiService
    {
        private static readonly string ApiKey = "1aa634bc0231b4af1904ccd51ed1604a";
        private static readonly HttpClient httpClient = new HttpClient();

        public static async Task<Root?> GetWeatherByLocation()
        {
            var location = await LocationService.GetCurrentLocationAsync();

            if (location != null)
            {
                var url = $"https://api.openweathermap.org/data/2.5/forecast?lat={location.Latitude}&lon={location.Longitude}&units=metric&appid={ApiKey}";
                var response = await httpClient.GetStringAsync(url);
                return JsonConvert.DeserializeObject<Root>(response);
            }
            else
            {
                await Shell.Current.DisplayAlert("Error", "Failed to determine your location. Please enable location access.", "OK");
                return null;
            }
        }

        public static async Task<Root?> GetWeatherByCityName(string cityName)
        {
            var url = $"https://api.openweathermap.org/data/2.5/forecast?q={cityName}&units=metric&appid={ApiKey}";
            var response = await httpClient.GetStringAsync(url);

            if (response != null)
            {
                return JsonConvert.DeserializeObject<Root>(response);
            }
            else
            {
                await Shell.Current.DisplayAlert("Error", "Something went wrong!", "OK");
                return null;
            }
        }
    }
}
