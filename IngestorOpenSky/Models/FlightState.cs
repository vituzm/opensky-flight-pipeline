using System.Text.Json;

namespace IngestorOpenSky.Models;
public class FlightState
{
    public string Icao24 { get; set; } = string.Empty;
    public string Callsign { get; set; } = string.Empty;
    public string OriginCountry { get; set; } = string.Empty;
    public int? TimePosition { get; set; }
    public int? LastContact { get; set; }
    public float? Longitude { get; set; }
    public float? Latitude { get; set; }
    public float? BaroAltitude { get; set; }
    public bool OnGround { get; set; }
    public float? Velocity { get; set; }
    public float? TrueTrack { get; set; }
    public float? VerticalRate { get; set; }
    public int[]? Sensors { get; set; }
    public float? GeoAltitude { get; set; }
    public string? Squawk { get; set; }
    public bool Spi { get; set; }
    public int PositionSource { get; set; }
    public int Category { get; set; }

    public static FlightState MapearDoArray(object?[] data)
    {
        return new FlightState
        {
            Icao24 = data[0]?.ToString() ?? string.Empty,
            Callsign = data[1]?.ToString()?.Trim() ?? string.Empty,
            OriginCountry = data[2]?.ToString() ?? string.Empty,
            
            TimePosition = ParseInt(data[3]),
            LastContact = ParseInt(data[4]),
            
            Longitude = ParseFloat(data[5]),
            Latitude = ParseFloat(data[6]),
            BaroAltitude = ParseFloat(data[7]),
            
            OnGround = data[8]?.ToString()?.ToLower() == "true",
            
            Velocity = ParseFloat(data[9]),
            TrueTrack = ParseFloat(data[10]),
            VerticalRate = ParseFloat(data[11]),
            
            Sensors = data[12] is JsonElement jsonElement
                ? JsonSerializer.Deserialize<int[]>(jsonElement.GetRawText())
                : null, 
            
            GeoAltitude = ParseFloat(data[13]),
            Squawk = data[14]?.ToString(),
            Spi = data[15]?.ToString()?.ToLower() == "true",
            
            PositionSource = ParseInt(data[16]) ?? 0,
            Category = ParseInt(data[17]) ?? 0
        };
    }

    // Métodos auxiliares para a conversão não quebrar o código
    private static float? ParseFloat(object? value) =>
        float.TryParse(value?.ToString(), out float result) ? result : null;

    private static int? ParseInt(object? value) =>
        int.TryParse(value?.ToString(), out int result) ? result : null;
}