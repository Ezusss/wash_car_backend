using Microsoft.AspNetCore.Mvc;
using WashCarApi.Models;
using WashCarApi.Services;
using System.Text.Json;

namespace WashCarApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class WeatherController : ControllerBase
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;
    private readonly WashRecommendationService _washService;

    public WeatherController(
        IHttpClientFactory httpClientFactory,
        IConfiguration configuration,
        WashRecommendationService washService)
    {
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
        _washService = washService;
    }

    [HttpGet("forecast")]
    public async Task<IActionResult> GetForecast(
        [FromQuery] string q, [FromQuery] int days = 10, [FromQuery] string aqi = "no")
    {
        if (string.IsNullOrWhiteSpace(q))
            return BadRequest("Location not specified");

        var apiKey = _configuration["WeatherApi__ApiKey"] ?? _configuration["WeatherApi:ApiKey"];
        var client = _httpClientFactory.CreateClient();
        var url = $"https://api.weatherapi.com/v1/forecast.json?key={apiKey}&q={q}&days={days}&aqi={aqi}";

        try
        {
            var response = await client.GetAsync(url);
            var content = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
                return StatusCode((int)response.StatusCode, content);

            using var doc = JsonDocument.Parse(content);
            var root = doc.RootElement;

            var location = root.GetProperty("location");
            var current = root.GetProperty("current");
            var currentCondition = current.GetProperty("condition");
            var forecastDays = root.GetProperty("forecast")
                                   .GetProperty("forecastday")
                                   .EnumerateArray().ToList();

            int currentCode = currentCondition.GetProperty("code").GetInt32();
            bool isRaining = _washService.IsRainCode(currentCode);
            bool isSnowing = _washService.IsSnowCode(currentCode);
            bool isThunder = _washService.IsThunderstormCode(currentCode);
            double windKph = current.GetProperty("wind_kph").GetDouble();
            int humidity = current.GetProperty("humidity").GetInt32();
            string conditionText = currentCondition.GetProperty("text").GetString() ?? "";

            int todayRainChance = forecastDays.Count > 0
                ? forecastDays[0].GetProperty("day").GetProperty("daily_chance_of_rain").GetInt32()
                : 0;

            var forecastDtos = new List<DayForecastDto>();
            var overallDays = new List<(int, bool, bool, double)>();

            foreach (var dayElem in forecastDays)
            {
                var day = dayElem.GetProperty("day");
                var dayCondition = day.GetProperty("condition");
                int dayCode = dayCondition.GetProperty("code").GetInt32();
                int rainChance = day.GetProperty("daily_chance_of_rain").GetInt32();
                int snowChance = day.GetProperty("daily_chance_of_snow").GetInt32();
                double avgHumidity = day.GetProperty("avghumidity").GetDouble();
                double maxWindKphDay = day.GetProperty("maxwind_kph").GetDouble();
                bool hasRain = rainChance >= 20;
                bool hasSnow = snowChance >= 20;

                int score = _washService.CalculateScoreForDay(
                    rainChance, snowChance, avgHumidity, maxWindKphDay, dayCode);

                forecastDtos.Add(new DayForecastDto
                {
                    Date = dayElem.GetProperty("date").GetString() ?? "",
                    Score = score,
                    Status = _washService.GetStatus(score),
                    MaxTempC = day.GetProperty("maxtemp_c").GetDouble(),
                    MinTempC = day.GetProperty("mintemp_c").GetDouble(),
                    AvgHumidity = avgHumidity,
                    MaxWindKph = maxWindKphDay,
                    RainChance = rainChance,
                    RainMm = day.GetProperty("totalprecip_mm").GetDouble(),
                    ConditionText = dayCondition.GetProperty("text").GetString() ?? "",
                    ConditionCode = dayCode,
                    HasRain = hasRain,
                    HasSnow = hasSnow,
                });

                overallDays.Add((rainChance, hasRain, hasSnow, maxWindKphDay));
            }

            int overallScore = _washService.CalculateOverallScore(
                isRaining, isSnowing, isThunder, windKph, humidity, conditionText, overallDays);

            return Ok(new WeatherForecastResponse
            {
                City = location.GetProperty("name").GetString() ?? "",
                Country = location.GetProperty("country").GetString() ?? "",
                Latitude = location.GetProperty("lat").GetDouble(),
                Longitude = location.GetProperty("lon").GetDouble(),
                WashScore = overallScore,
                WashStatus = _washService.GetStatus(overallScore),
                Current = new CurrentWeatherDto
                {
                    TempC = current.GetProperty("temp_c").GetDouble(),
                    TempF = current.GetProperty("temp_f").GetDouble(),
                    Humidity = humidity,
                    WindKph = windKph,
                    RainChance = todayRainChance,
                    ConditionText = conditionText,
                    ConditionCode = currentCode,
                    UvIndex = current.GetProperty("uv").GetDouble(),
                    IsRaining = isRaining,
                    IsSnowing = isSnowing,
                },
                Forecast = forecastDtos,
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, ex.Message);
        }
    }
}