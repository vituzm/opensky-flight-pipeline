public class OpenSkyOptions
{
    public string OpenSkyUrl { get; set; } = string.Empty;
    public BoundingBox BoundingBox { get; set; } = new();
}

public class BoundingBox
{
    public string Lamin { get; set; } = string.Empty;
    public string Lomin { get; set; } = string.Empty;
    public string Lamax { get; set; } = string.Empty;
    public string Lomax { get; set; } = string.Empty;
}