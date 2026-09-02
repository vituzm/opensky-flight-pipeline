namespace IngestorOpenSky.Models;

public class KafkaEvent
{
    public string Topic { get; set; } = string.Empty; 
    public string Key { get; set; } = string.Empty;
    public byte[] Value { get; set; } = Array.Empty<byte>();
    public Dictionary<string, byte[]> Headers { get; set; } = new();
}