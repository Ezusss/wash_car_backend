namespace WashCarApi.Models;

public class DayForecast
{
    public string Date { get; set; } = string.Empty;
    public int Score { get; set; }
    public string Recommendation { get; set; } = string.Empty;
    public double MaxTemp { get; set; }
    public double MinTemp { get; set; }
    public int ChanceOfRain { get; set; }
    public string Condition { get; set; } = string.Empty;
    public string Icon { get; set; } = string.Empty;
}

public class WeatherForecastResponse
{
    public string City { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public List<DayForecast> Forecast { get; set; } = new();
}