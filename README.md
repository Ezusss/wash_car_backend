# Wash Car API

ASP.NET Core backend for the [When To Wash A Car?](https://github.com/Ezusss/wash_car_app) Flutter app.

Acts as a secure proxy between the mobile app and WeatherAPI.com — keeps the API key on the server side.

## Tech Stack

- ASP.NET Core (.NET 10)
- C#
- REST API
- Deployed on Railway

## Endpoints

### GET /api/weather/forecast

Returns weather forecast for a given location.

**Query parameters:**

| Parameter | Type | Description |
|---|---|---|
| q | string | City name or coordinates (lat,lon) |
| days | int | Number of forecast days (default: 10) |
| aqi | string | Air quality index (default: no) |

**Example request:**

```
GET /api/weather/forecast?q=Vladivostok&days=7
```

**Example response:**

```json
{
  "location": { "name": "Vladivostok" },
  "current": { "temp_c": 12.6 },
  "forecast": { "forecastday": [] }
}
```

## Project Structure

```
WashCarApi/
├── Controllers/
│   └── WeatherController.cs         # API endpoints
├── Models/
│   └── WeatherResponse.cs           # Response models
├── Services/
│   └── WashRecommendationService.cs # Scoring logic
├── Program.cs                       # App configuration
└── appsettings.json
```

## Getting Started

### Prerequisites

- .NET 10 SDK
- WeatherAPI.com API key (free tier available)

### Run locally

```bash
git clone https://github.com/Ezusss/wash_car_backend.git
cd wash_car_backend/WashCarApi
```

Add your API key to `appsettings.json`:

```json
{
  "WeatherApi": {
    "ApiKey": "your_api_key_here"
  }
}
```

```bash
dotnet run
```

API will be available at `http://localhost:5059`

### Environment Variables

For production, set the API key via environment variable:

```
WeatherApi__ApiKey=your_api_key_here
```

## License

MIT
