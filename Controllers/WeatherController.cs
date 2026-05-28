using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using WashCarApi.Models;
using WashCarApi.Services;

namespace WashCarApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class WeatherController : ControllerBase
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;
    private readonly WashRecommendationService _recommendationService;

    public WeatherController(IHttpClientFactory httpClientFactory, IConfiguration configuration)
    {
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
        _recommendationService = new WashRecommendationService();
    }

    [HttpGet("forecast")]
    public async Task<IActionResult> GetForecast([FromQuery] string city)
    {
        if (string.IsNullOrWhiteSpace(city))
            return BadRequest("Город не указан");

        var apiKey = _configuration["WeatherApi:ApiKey"];
        var client = _httpClientFactory.CreateClient();

        var url = $"https://api.weatherapi.com/v1/forecast.json?key={apiKey}&q={city}&days=7&lang=ru";

        try
        {
            var response = await client.GetAsync(url);
            var content = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
                return StatusCode((int)response.StatusCode, content);

            var json = JsonDocument.Parse(content);
            var root = json.RootElement;

            var result = new WeatherForecastResponse
            {
                City = root.GetProperty("location").GetProperty("name").GetString() ?? city,
                Country = root.GetProperty("location").GetProperty("country").GetString() ?? "",
                Forecast = new List<DayForecast>()
            };

            foreach (var day in root.GetProperty("forecast").GetProperty("forecastday").EnumerateArray())
            {
                var dayData = day.GetProperty("day");
                var condition = dayData.GetProperty("condition");

                var score = _recommendationService.CalculateScore(dayData, condition);

                result.Forecast.Add(new DayForecast
                {
                    Date = day.GetProperty("date").GetString() ?? "",
                    Score = score,
                    Recommendation = _recommendationService.GetRecommendation(score),
                    MaxTemp = dayData.GetProperty("maxtemp_c").GetDouble(),
                    MinTemp = dayData.GetProperty("mintemp_c").GetDouble(),
                    ChanceOfRain = dayData.GetProperty("daily_chance_of_rain").GetInt32(),
                    Condition = condition.GetProperty("text").GetString() ?? "",
                    Icon = condition.GetProperty("icon").GetString() ?? ""
                });
            }

            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, ex.Message);
        }
    }
}