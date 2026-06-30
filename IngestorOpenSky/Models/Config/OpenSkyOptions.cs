public class OpenSkyOptions
{
    public string OpenSkyUrl { get; set; } = string.Empty;
    public BoundingBox BoundingBox { get; set; } = new();
    public OpenSkyParameters OpenSkyParameters { get; set; } = new();
    public string OpenSkyTopic { get; set; } = string.Empty;
    public int MaxRetries { get; set; } = 0;

    public Dictionary<string, string?> ToQueryDictionary()
    {
        return new Dictionary<string, string?>
        {
            ["time"]    = OpenSkyParameters.Time,
            ["icao24"]  = OpenSkyParameters.Icao24,
            ["extended"]= OpenSkyParameters.Extended,
            ["lamin"]   = BoundingBox.Lamin,
            ["lomin"]   = BoundingBox.Lomin,
            ["lamax"]   = BoundingBox.Lamax,
            ["lomax"]   = BoundingBox.Lomax,
        };
    }
}

public class BoundingBox
{
    public string Lamin { get; set; } = string.Empty;
    public string Lomin { get; set; } = string.Empty;
    public string Lamax { get; set; } = string.Empty;
    public string Lomax { get; set; } = string.Empty;
}

public class OpenSkyParameters
{
    public string? Time { get; set; }
    public string? Icao24 { get; set; }
    public string Extended { get; set; } = string.Empty;
}