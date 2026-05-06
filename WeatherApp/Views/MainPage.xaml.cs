using System.Collections.ObjectModel;
using WeatherApp.Models;
using WeatherApp.Services;

namespace WeatherApp.Views;

public partial class MainPage : ContentPage
{
    public ObservableCollection<ForecastDay> _listOfForecastDays { get; set; } = new ObservableCollection<ForecastDay>();
    private bool _isFirstLoad = true;

    public MainPage()
    {
        InitializeComponent();
        forecastCollection.ItemsSource = _listOfForecastDays;
        this.BindingContext = this;
    }

    protected async override void OnAppearing()
    {
        base.OnAppearing();
        if (_isFirstLoad && (_listOfForecastDays.Count == 0))
        {
            await RefreshWeatherData();
            _isFirstLoad = false;
        }
    }

    private async Task RefreshWeatherData()
    {
        if (Connectivity.Current.NetworkAccess != NetworkAccess.Internet)
        {
            await DisplayAlertAsync("Connection error", "No internet access.", "OK");
            return;
        }

        var result = await ApiService.GetWeatherByLocation();
        if (result != null)
        {
            await UpdateWeatherUI(result);
        }
        mainRefreshView.IsRefreshing = false;
    }

    private async void OnRefreshing(object sender, EventArgs e) => await RefreshWeatherData();

    private async Task UpdateWeatherUI(Root result)
    {
        if (result?.list == null) return;

        var grouped = result.list.GroupBy(f => f.DayOfWeek).Select(g => new ForecastDay
        {
            DayName = g.First().DayOfWeek,
            Date = g.First().DateFormatted,
            Items = g.ToList()
        }).Skip(1).ToList();

        _listOfForecastDays.Clear();
        foreach (var day in grouped)
        {
            _listOfForecastDays.Add(day);
        }

        forecastCollection.ItemsSource = result.list.Take(8).ToList();

        lblCity.Text = result.city?.name;
        lblTemperature.Text = result.list[0].main?.temperature.ToString("0") + "°";
        lblWeatherDescription.Text = result.list[0].weather?[0]?.description;
        lblHunidity.Text = result.list[0].main?.humidity + " %";
        lblWind.Text = (result.list[0].wind.speed * 3.6).ToString("0") + " km/h";
        ImgWeatherIcon.Source = result.list[0].weather?[0]?.customIcon;
        lblFeelsLike.Text = result.list[0].main?.FeelsLike.ToString() + "°";

        await Task.Yield();
    }

    private async void OnSearchClicked(object sender, EventArgs e)
    {
        // A MAUI beépített promptja: Cím, Üzenet, OK gomb szövege, Mégse gomb szövege
        string cityName = await DisplayPromptAsync("City Search", "Serch by city name", "Search", "Cancel", "e.g. London");

        if (!string.IsNullOrWhiteSpace(cityName))
        {
            try
            {
                // Meghívjuk az ApiService-t (azt, amit korábban mutattál)
                var result = await ApiService.GetWeatherByCityName(cityName);

                if (result != null)
                {
                    // Frissítjük a kijelzõt az új adatokkal
                    await UpdateWeatherUI(result);
                }
                else
                {
                    await DisplayAlertAsync("Error", "City not found. Please check the spelling!", "OK");
                }
            }
            catch (Exception)
            {
                await DisplayAlertAsync("Error", "Failed to load weather data.", "OK");
            }
        }
    }

    private async void OnSevenDaysTapped(object sender, EventArgs e)
    {
        if (_listOfForecastDays.Count > 0)
        {
            await Navigation.PushAsync(new ForecastPage(lblCity.Text) { BindingContext = this });
        }
    }
}