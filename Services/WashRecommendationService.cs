namespace WashCarApi.Services;

public class WashRecommendationService
{
    private static readonly HashSet<int> RainCodes = new()
    {
        1063, 1150, 1153, 1168, 1171, 1180, 1183, 1186,
        1189, 1192, 1195, 1198, 1201, 1240, 1243, 1246, 1249, 1252
    };

    private static readonly HashSet<int> SnowCodes = new()
    {
        1066, 1114, 1117, 1210, 1213, 1216, 1219, 1222,
        1225, 1255, 1258, 1279, 1282
    };

    private static readonly HashSet<int> ThunderstormCodes = new()
    {
        1087, 1273, 1276, 1279, 1282
    };

    private const double MaxRainProbability = 20.0;
    private const double MaxWindKph = 43.2; // 12 m/s

    public bool IsRainCode(int code) => RainCodes.Contains(code);
    public bool IsSnowCode(int code) => SnowCodes.Contains(code);
    public bool IsThunderstormCode(int code) => ThunderstormCodes.Contains(code);

    public int CalculateScoreForDay(
        int rainChance, int snowChance, double avgHumidity,
        double maxWindKph, int conditionCode)
    {
        int score = 50;

        bool hasRain = rainChance >= MaxRainProbability;
        bool hasSnow = snowChance >= MaxRainProbability;
        bool isThunder = ThunderstormCodes.Contains(conditionCode);

        if (!hasRain) score += 20;
        if (!hasSnow) score += 10;
        if (maxWindKph < MaxWindKph) score += 15;
        if (avgHumidity < 70) score += 10;
        if (rainChance < 10) score += 15;

        if (hasRain) score -= 40;
        if (hasSnow) score -= 30;
        if (isThunder) score -= 70;
        if (maxWindKph >= MaxWindKph) score -= 15;
        if (avgHumidity >= 80) score -= 10;
        if (rainChance >= 50) score -= 20;

        return Math.Clamp(score, 0, 100);
    }

    public int CalculateOverallScore(
        bool isRaining, bool isSnowing, bool isThunder,
        double windKph, int humidity, string conditionText,
        List<(int rainChance, bool hasRain, bool hasSnow, double maxWindKph)> days)
    {
        int score = 0;

        if (!isRaining) score += 10;
        if (!isSnowing) score += 10;
        if (windKph < MaxWindKph) score += 10;
        if (humidity < 70) score += 10;

        if (days.Count > 0)
        {
            var today = days[0];
            if (!today.hasRain) score += 40;
            if (!today.hasSnow) score += 5;
            if (today.maxWindKph < MaxWindKph) score += 10;

            if (days.Count > 2)
            {
                if (!days[1].hasRain && !days[2].hasRain) score += 25;
            }

            if (today.hasRain) score -= 50;
            if (today.hasSnow) score -= 40;
        }

        if (conditionText.Contains("Sunny") || conditionText.Contains("Clear") ||
            conditionText.Contains("Partly cloudy")) score += 15;
        if (isRaining) score -= 50;
        if (isSnowing) score -= 40;
        if (isThunder) score -= 70;
        if (conditionText.Contains("Dust") || conditionText.Contains("Sand")) score -= 20;

        return Math.Clamp(score, 0, 100);
    }

    public string GetStatus(int score)
    {
        if (score >= 70) return "safe";
        if (score >= 40) return "warning";
        return "unsafe";
    }
}