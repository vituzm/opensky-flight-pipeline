namespace IngestorOpenSky.Models;

public class KafkaEvent
{
    public string Topic { get; set; } = "flight-data"; // Nome do tópico Kafka
    public string Key { get; set; } = string.Empty; // Ex: Icao24
    public string Value { get; set; } = string.Empty; // Ex: O JSON do FlightState
    public Dictionary<string, byte[]> Headers { get; set; } = new();
}