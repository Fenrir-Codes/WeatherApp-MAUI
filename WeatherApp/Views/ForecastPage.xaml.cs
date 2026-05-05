namespace WeatherApp.Views;

public partial class ForecastPage : ContentPage
{
	public ForecastPage()
	{
		InitializeComponent();
	}

    private async void OnBackClicked(object sender, EventArgs e)
    {
        await Navigation.PopAsync();
    }

    public ForecastPage(string cityName)
    {
        InitializeComponent();
        lblCityName.Text = cityName;
    }
}