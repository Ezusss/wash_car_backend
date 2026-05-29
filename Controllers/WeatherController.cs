using Microsoft.AspNetCore.Mvc;
using WashCarApi.Services;
using System.Text.Json;

namespace WashCarApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class WeatherController : ControllerBase
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;

    public WeatherController(IHttpClientFactory httpClientFactory, IConfiguration configuration)
    {
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
    }

    [HttpGet("forecast")]
    public async Task<IActionResult> GetForecast([FromQuery] string q, [FromQuery] int days = 10, [FromQuery] string aqi = "no")
    {
        if (string.IsNullOrWhiteSpace(q))
            return BadRequest("Локация не указана");

        var apiKey = _configuration["WeatherApi__ApiKey"] ?? _configuration["WeatherApi:ApiKey"];
        var client = _httpClientFactory.CreateClient();

        var url = $"https://api.weatherapi.com/v1/forecast.json?key={apiKey}&q={q}&days={days}&aqi={aqi}";

        try
        {
            var response = await client.GetAsync(url);
            var content = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
                return StatusCode((int)response.StatusCode, content);

            return Content(content, "application/json");
        }
        catch (Exception ex)
        {
            return StatusCode(500, ex.Message);
        }
    }
}