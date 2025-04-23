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
        loadingSpinner.IsVisible = true;
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
            loadingSpinner.IsVisible = false;
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
        catch (HttpRequestException ex)
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
    private async Task UpdateWeatherUI(dynamic result)
    {
        _listOfWeather.Clear();
        foreach (var item in result.list)
        {
            _listOfWeather.Add(item);
        }
        WeatherCollection.ItemsSource = _listOfWeather;

        lblCity.Text = result.city?.name + ", " + result.city?.country ?? "Unknown City";
        lblWeatherDescription.Text = result.list[0].weather?[0]?.description ?? "No description";
        lblTemperature.Text = $"{result.list[0].main?.temperature ?? "N/A"}°C";
        lblFeelsLike.Text = $"{result.list[0].main?.FeelsLike ?? "N/A"}°C";
        lblHunidity.Text = result.list[0].main?.humidity + "%" ?? "N/A";
        lblWind.Text = (result.list[0].wind.speed * 3.6).ToString("0") + " km/h";
        ImgWeatherIcon.Source = result.list[0].weather?[0]?.customIcon ?? string.Empty;

        await Task.Yield();
    }
    #endregion

    #region Tapping the the location button
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
    private async void ImageButton_Clicked(object sender, EventArgs e)
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