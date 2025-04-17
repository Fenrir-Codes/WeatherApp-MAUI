namespace WeatherApp.Services
{
    internal static class LocationService
    {
        public static async Task<Location?> GetCurrentLocationAsync()
        {
            try
            {
                var geodata = await Geolocation.GetLocationAsync(new GeolocationRequest
                {
                    DesiredAccuracy = GeolocationAccuracy.High,
                    Timeout = TimeSpan.FromSeconds(10)
                });
                return geodata;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error while getting geodata: " + ex.Message);
                return null;
            }
        }
    }
}
