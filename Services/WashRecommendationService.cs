using System.Text.Json;

namespace WashCarApi.Services;

public class WashRecommendationService
{
    public int CalculateScore(JsonElement dayData, JsonElement condition)
    {
        int score = 0;

        int chanceOfRain = dayData.GetProperty("daily_chance_of_rain").GetInt32();
        if (chanceOfRain == 0) score += 40;
        else if (chanceOfRain < 30) score += 20;
        else if (chanceOfRain >= 70) score -= 50;

        int chanceOfSnow = dayData.GetProperty("daily_chance_of_snow").GetInt32();
        if (chanceOfSnow > 20) score -= 40;

        int humidity = dayData.GetProperty("avghumidity").GetInt32();
        if (humidity < 60) score += 10;
        else if (humidity > 85) score -= 10;

        double wind = dayData.GetProperty("maxwind_kph").GetDouble();
        if (wind < 20) score += 10;
        else if (wind > 40) score -= 10;

        int code = condition.GetProperty("code").GetInt32();
        if (code == 1000 || code == 1003) score += 15;
        if (code == 1087 || code == 1273 || code == 1276) score -= 70;

        return Math.Clamp(score, 0, 100);
    }

    public string GetRecommendation(int score)
    {
        if (score >= 80) return "Отличное время для мойки";
        if (score >= 50) return "Можно помыть, но не идеально";
        return "Не стоит мыть машину";
    }
}