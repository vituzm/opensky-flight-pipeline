namespace IngestorOpenSky.Models;
using System.Text;

class FlightDataMapper
{
    public List<KafkaEvent> MapToKafkaEvents(OpenSkyResponse response, Dictionary<string, string?> parametros)
    {

        List<KafkaEvent> eventos = new List<KafkaEvent>(); 
        var dicionarioHeader = BuildHeaders(response.Time.ToString(), parametros);

        foreach (var voo in response.States)
        {
            var vooString = SerializaVoo(voo);
            var kafkaEvent = new KafkaEvent
            {
                Key = voo.Icao24, // Identificador único do avião
                Value = vooString,
                Headers = dicionarioHeader
            };

            eventos.Add(kafkaEvent);
        }

        return eventos;
    }

    
    private static Dictionary<string, byte[]> BuildHeaders(string unixTimestamp, Dictionary<string, string?> parametros)
    {
        var headers = new Dictionary<string, byte[]>
        {
            { "api_unix_time", Encoding.UTF8.GetBytes(unixTimestamp)}, // E se o dado for nulo? 
            { "source", Encoding.UTF8.GetBytes("opensky_api")}
        };

        foreach (var param in parametros)
        {
            if (param.Value != null)
            {
                headers.Add(param.Key, Encoding.UTF8.GetBytes(param.Value));
            }
        }

        return headers;
    }

    private static string SerializaVoo(FlightState voo)
    {
        return System.Text.Json.JsonSerializer.Serialize(voo);
    }

}