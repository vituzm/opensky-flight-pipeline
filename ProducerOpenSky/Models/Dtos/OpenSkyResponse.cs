using System.Text.Json.Serialization;

using ProducerOpenSky.Models;

namespace ProducerOpenSky.Models;

public class OpenSkyResponse
{
    [JsonPropertyName("time")]
    public int Time { get; set; }

    [JsonPropertyName("states")]
    public List<object?[]>? StatesRaw { get; set; }

    [JsonIgnore] 
    public List<FlightState> States { get; set; } = new();
}