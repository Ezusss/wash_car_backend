namespace WashCarApi.Models;

public class CurrentWeatherDto
{
    public double TempC { get; set; }
    public double TempF { get; set; }
    public int Humidity { get; set; }
    public double WindKph { get; set; }
    public int RainChance { get; set; }
    public string ConditionText { get; set; } = string.Empty;
    public int ConditionCode { get; set; }
    public double UvIndex { get; set; }
    public bool IsRaining { get; set; }
    public bool IsSnowing { get; set; }
}

public class DayForecastDto
{
    public string Date { get; set; } = string.Empty;
    public int Score { get; set; }
    public string Status { get; set; } = string.Empty;
    public double MaxTempC { get; set; }
    public double MinTempC { get; set; }
    public double AvgHumidity { get; set; }
    public double MaxWindKph { get; set; }
    public double RainChance { get; set; }
    public double RainMm { get; set; }
    public string ConditionText { get; set; } = string.Empty;
    public int ConditionCode { get; set; }
    public bool HasRain { get; set; }
    public bool HasSnow { get; set; }
}

public class WeatherForecastResponse
{
    public string City { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public int WashScore { get; set; }
    public string WashStatus { get; set; } = string.Empty;
    public CurrentWeatherDto Current { get; set; } = new();
    public List<DayForecastDto> Forecast { get; set; } = new();
}