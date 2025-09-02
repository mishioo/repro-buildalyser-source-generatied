using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Example;

public class WeatherForecast
{
    public DateTime Date { get; set; }
    public int TemperatureCelsius { get; set; }
    public string? Summary { get; set; }
}

[JsonSerializable(typeof(WeatherForecast))]
internal partial class SourceGenerationContext : JsonSerializerContext { }

public class Service
{
    public WeatherForecast Deserialize(string json)
    {
        return JsonSerializer.Deserialize(json, SourceGenerationContext.Default.WeatherForecast)!;
    }
}
