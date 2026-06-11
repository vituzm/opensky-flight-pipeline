namespace IngestorOpenSky.Models;

public class KafkaEvent
{
    public string Topic { get; set; } = string.Empty; 
    public string Key { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public Dictionary<string, byte[]> Headers { get; set; } = new();
}