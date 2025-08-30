using System.Collections.ObjectModel;
using WeatherApp.Models;
using WeatherApp.Services;

namespace WeatherApp.Views;

public partial class MainPage : ContentPage
{
    private ObservableCollection<ForecastDay> _listOfForecastDays;

    public MainPage()
    {
        InitializeComponent();
        loadingSpinner.IsVisible = true;

        // Üres lista induláskor
        _listOfForecastDays = new ObservableCollection<ForecastDay>();
        forecastCollection.ItemsSource = _listOfForecastDays;
    }

    protected async override void OnAppearing()
    {
        base.OnAppearing();

        //Checking network connectivity
        if (Connectivity.Current.NetworkAccess != NetworkAccess.Internet)
        {
            await DisplayAlert("Connection error", "You are currrently not connetcting to any network.", "OK");
            return;
        }

        var result = await ApiService.GetWeatherByLocation();

        if (result != null)
        {
            await GetWeatherOnStart(result);
            loadingSpinner.IsVisible = false;
        }
        else
        {
            await DisplayAlert("Error", "Error while fetching weather data!", "OK");
        }
    }

    #region Getting the weather on start
    private async Task GetWeatherOnStart(Root result)
    {
        if (result != null)
        {
            await UpdateWeatherUI(result);
            loadingSpinner.IsVisible = false;
        }
        else
        {
            await DisplayAlert("Error", "Error while fetching weather data!", "OK");
        }
    }
    #endregion

    #region Getting the data by city name
    private async Task GetWeatherByCityName(string cityName)
    {
        try
        {
            var result = await ApiService.GetWeatherByCityName(cityName);

            if (result != null)
            {
                await UpdateWeatherUI(result);
                loadingSpinner.IsVisible = false;
            }
            else
            {
                await DisplayAlert("City Not Found", "No weather data found for the specified city name.", "OK");
            }
        }
        catch (HttpRequestException)
        {
            await DisplayAlert("Error", "City name not found!", "OK");
        }
        catch (Exception ex)
        {
            await DisplayAlert("Unexpected Error", $"An error occurred: {ex.Message}", "OK");
        }
    }
    #endregion

    #region Updating the weather data on UI
    private async Task UpdateWeatherUI(Root result)
    {
        if (result?.list == null) return;

        // Csoportosítás napokra
        var grouped = result.list
            .GroupBy(f => f.DayOfWeek)
            .Select(g => new ForecastDay
            {
                DayName = g.First().DayOfWeek,
                Date = g.First().DateFormatted,
                Items = g.ToList()
            })
            .ToList();

        _listOfForecastDays.Clear();
        foreach (var day in grouped)
        {
            _listOfForecastDays.Add(day);
        }

        // Felsõ címké, aktuális idõjárás a legelsõ itembõl
        lblCity.Text = result.city?.name + ", " + result.city?.country ?? "Unknown City";
        lblWeatherDescription.Text = result.list[0].weather?[0]?.description ?? "No description";
        lblTemperature.Text = $"{(result.list[0].main?.temperature.ToString("0") ?? "N/A")}°C";
        lblFeelsLike.Text = $"{result.list[0].main?.FeelsLike ?? "N/A"}°C";
        lblHunidity.Text = result.list[0].main?.humidity + "%" ?? "N/A";
        lblWind.Text = (result.list[0].wind.speed * 3.6).ToString("0") + " km/h";
        ImgWeatherIcon.Source = result.list[0].weather?[0]?.customIcon ?? string.Empty;

        await Task.Yield();
    }
    #endregion

    #region Tapping the location button
    private async void GetMyCurrentLocation(object sender, EventArgs e)
    {
        try
        {
            var result = await ApiService.GetWeatherByLocation();

            if (result != null)
            {
                await UpdateWeatherUI(result);
                loadingSpinner.IsVisible = false;
            }
            else
            {
                await DisplayAlert("Location Error", "Unable to fetch weather data for your location.", "OK");
            }
        }
        catch (HttpRequestException)
        {
            await DisplayAlert("Network Error", "Unable to reach the weather service.", "OK");
        }
        catch (Exception ex)
        {
            await DisplayAlert("Unexpected Error", $"An error occurred: {ex.Message}", "OK");
        }
    }
    #endregion

    #region Getting the weather by city name
    private async void SearchLocation(object sender, EventArgs e)
    {
        var cityName = await DisplayPromptAsync(title: "Search City", message: "", placeholder: "Search weather by city name", accept: "Search", cancel: "Cancel");

        if (!string.IsNullOrWhiteSpace(cityName))
        {
            await GetWeatherByCityName(cityName);
            loadingSpinner.IsVisible = false;
        }
        else if (cityName == null)
        {
            return;
        }
        else
        {
            await DisplayAlert("Error", "City name cannot be empty.", "OK");
        }
    }
    #endregion
}
