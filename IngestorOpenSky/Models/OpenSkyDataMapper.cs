namespace IngestorOpenSky.Models;
using IngestorOpenSky.Interfaces;
using System.Text;

class OpenSkyDataMapper : IOpenSkyDataMapper
{
    public List<KafkaEvent> MapToKafkaEvents(OpenSkyResponse response, Dictionary<string, string?> parameters)
    {

        List<KafkaEvent> eventos = new List<KafkaEvent>(); 
        var dicionarioHeader = BuildHeaders(response.Time.ToString(), parameters);

        foreach (var voo in response.States)
        {
            var vooString = SerializeFlight(voo);
            var kafkaEvent = new KafkaEvent
            {
                Key = voo.Icao24, // ID from the plane
                Value = vooString,
                Headers = dicionarioHeader
            };

            eventos.Add(kafkaEvent);
        }

        return eventos;
    }

    
    private static Dictionary<string, byte[]> BuildHeaders(string unixTimestamp, Dictionary<string, string?> parameters)
    {
        var headers = new Dictionary<string, byte[]>
        {
            { "api_unix_time", Encoding.UTF8.GetBytes(unixTimestamp)},
            { "source", Encoding.UTF8.GetBytes("opensky_api")}
        };

        foreach (var param in parameters)
        {
            if (param.Value != null)
            {
                headers.Add(param.Key, Encoding.UTF8.GetBytes(param.Value));
            }
        }

        return headers;
    }

    private static string SerializeFlight(FlightState voo)
    {
        return System.Text.Json.JsonSerializer.Serialize(voo);
    }

}