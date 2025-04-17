using System.Collections.ObjectModel;
using WeatherApp.Models;
using WeatherApp.Services;

namespace WeatherApp;

public partial class WeatherPage : ContentPage
{
    public ObservableCollection<ForecastItem> _listOfWeather;

    public WeatherPage()
    {
        InitializeComponent();
        _listOfWeather = new ObservableCollection<ForecastItem>();
        WeatherCollection.ItemsSource = _listOfWeather;
    }

    protected async override void OnAppearing()
    {
        base.OnAppearing();

        //Checking network connectivity
        if (Connectivity.Current.NetworkAccess != NetworkAccess.Internet)
        {
            await DisplayAlert("No Internet", "There is no internet connection.", "OK");
            return;
        }

        var result = await ApiService.GetWeatherByLocation();

        if (result != null)
        {
            await GetWeatherOnStart(result);
        }
        else
        {
            await DisplayAlert("Error", "Error while fetching weather data!", "OK");
        }

    }

    #region Gettint the weather start
    private async Task GetWeatherOnStart(Root result)
    {
        if (result != null)
        {
            await UpdateWeatherUI(result);
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
        var result = await ApiService.GetWeatherByCityName(cityName);
        if (result != null)
        {
            await UpdateWeatherUI(result);
        }
        else
        {
            await DisplayAlert("Error", "Error while fetching weather data!", "OK");
        }
    }
    #endregion

    #region Updating the weather data on UI
    private async Task UpdateWeatherUI(dynamic result)
    {
        _listOfWeather.Clear();
        foreach (var item in result.list)
        {
            _listOfWeather.Add(item);
        }
        WeatherCollection.ItemsSource = _listOfWeather;

        lblCity.Text = result.city?.name ?? "Unknown City";
        lblWeatherDescription.Text = result.list[0].weather?[0]?.description ?? "No description";
        lblTemperature.Text = result.list[0].main?.temperature + "°C" ?? "N/A";
        lblHunidity.Text = result.list[0].main?.humidity + "%" ?? "N/A";
        lblWind.Text = result.list[0].wind?.speed + "km/h" ?? "N/A";
        ImgWeatherIcon.Source = result.list[0].weather?[0]?.customIcon ?? string.Empty;

        await Task.Yield();
    }
    #endregion

    #region Tapping the the location button
    private async void TapLocation_Tapped(object sender, EventArgs e)
    {
        var result = await ApiService.GetWeatherByLocation();

        if (result != null)
        {
            await UpdateWeatherUI(result);
        }
        else
        {
            await DisplayAlert("Error", "Error while fetching weather data!", "OK");
        }
    }
    #endregion

    #region Getting the weather by city name
    private async void ImageButton_Clicked(object sender, EventArgs e)
    {
        var cityName = await DisplayPromptAsync(title: "", message: "", placeholder: "Search weather by city name", accept: "Search", cancel: "Cancel");

        if (!string.IsNullOrWhiteSpace(cityName))
        {
            await GetWeatherByCityName(cityName);
        }
        else
        {
            await DisplayAlert("Error", "City name cannot be empty.", "OK");
        }
    }
    #endregion

}