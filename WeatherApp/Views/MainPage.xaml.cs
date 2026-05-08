using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using WeatherApp.Models;
using WeatherApp.Services;

namespace WeatherApp.Views;

public partial class MainPage : ContentPage, INotifyPropertyChanged
{
    private Root _weatherData;
    public Root WeatherData
    {
        get => _weatherData;
        set { _weatherData = value; OnPropertyChanged(); }
    }

    public ObservableCollection<ForecastDay> _listOfForecastDays { get; set; } = new ObservableCollection<ForecastDay>();
    private bool _isFirstLoad = true;

    public MainPage()
    {
        InitializeComponent();
        this.BindingContext = this;
    }

    protected async override void OnAppearing()
    {
        base.OnAppearing();
        if (_isFirstLoad)
        {
            await RefreshWeatherData();
            _isFirstLoad = false;
        }
    }

    private async Task RefreshWeatherData()
    {
        if (Connectivity.Current.NetworkAccess != NetworkAccess.Internet)
        {
            await DisplayAlertAsync("Error", "No internet access.", "OK");
            return;
        }

        var result = await ApiService.GetWeatherByLocation();
        if (result != null) UpdateWeatherUI(result);
        mainRefreshView.IsRefreshing = false;
    }

    private void UpdateWeatherUI(Root result)
    {
        if (result?.list == null) return;

        WeatherData = result;

        var grouped = result.list.GroupBy(f => f.DayOfWeek).Select(g => new ForecastDay
        {
            DayName = g.First().DayOfWeek,
            Date = g.First().DateFormatted,
            Items = g.ToList()
        }).Skip(1).ToList();

        _listOfForecastDays.Clear();
        foreach (var day in grouped) _listOfForecastDays.Add(day);

        forecastCollection.ItemsSource = result.list.Take(8).ToList();
    }

    private async void OnRefreshing(object sender, EventArgs e) => await RefreshWeatherData();

    private async void OnSearchClicked(object sender, EventArgs e)
    {
        string cityName = await DisplayPromptAsync("Search for city", "Type the name of the city:", "Search", "Cancel");

        if (!string.IsNullOrWhiteSpace(cityName))
        {
            var result = await ApiService.GetWeatherByCityName(cityName);
            if (result != null)
            {
                UpdateWeatherUI(result);
            }
        }
    }

    private async void OnSevenDaysTapped(object sender, EventArgs e)
    {
        if (WeatherData != null && _listOfForecastDays != null && _listOfForecastDays.Count > 0)
        {
            var forecastPage = new ForecastPage(WeatherData.city?.name);

            forecastPage.BindingContext = this;
            await Navigation.PushAsync(forecastPage);
        }
    }

    // PropertyChanged implementáció
    public event PropertyChangedEventHandler PropertyChanged;
    public void OnPropertyChanged([CallerMemberName] string name = "") =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}