namespace IngestorOpenSky.Models.Config;

public class KafkaOptions
{
    public string BootstrapServers { get; set; } = string.Empty;
    public int MessageTimeoutMs { get; set; } = 5000;
}