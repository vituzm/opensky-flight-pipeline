namespace ProducerOpenSky.Models;

using Confluent.Kafka;
using Confluent.SchemaRegistry;
using Confluent.SchemaRegistry.Serdes;
using ProducerOpenSky.Interfaces;
using System.Text;
using System.Text.Json;

class OpenSkyDataMapper : IOpenSkyDataMapper
{   
    private ProtobufSerializer<FlightState> _protobufSerializer;

    public OpenSkyDataMapper(ISchemaRegistryClient schemaRegistry)
    {
        _protobufSerializer = new(schemaRegistry);
    }


    public async Task<List<KafkaEvent>> MapToKafkaEvents(OpenSkyResponse response, Dictionary<string, string?> parameters, string topic)
    {

        List<KafkaEvent> events = new List<KafkaEvent>(); 
        var headerDict = BuildHeaders(response.Time.ToString(), parameters);
        var serializationContext = new SerializationContext(MessageComponentType.Value, topic);

        foreach (var rawFlight in response.StatesRaw ?? [])
        {
            FlightState flight = MapArrayFlightState(rawFlight);
            
            byte[] serializedFlight =  await _protobufSerializer.SerializeAsync(flight, serializationContext); // Serialização protobuf

            var kafkaEvent = new KafkaEvent
            {
                Topic = topic,
                Key = flight.Icao24, // ID from the plane
                Value = serializedFlight,
                Headers = headerDict
            };

            events.Add(kafkaEvent);
        }

        return events;
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

    private static FlightState MapArrayFlightState(object?[] data)
    {
        FlightState flight = new FlightState
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
            
            // Sensors = AddRange(data[12]),
            
            GeoAltitude = ParseFloat(data[13]),
            Squawk = data[14]?.ToString(),
            Spi = data[15]?.ToString()?.ToLower() == "true",
            
            PositionSource = ParseInt(data[16]) ?? 0,
            Category = ParseInt(data[17]) ?? 0
        };


        // Preenche os sensores com segurança apenas se o campo existir e for um array válido // Temos que adicionar os elementos depois da lista ser iniciada
        if (data[12] is JsonElement jsonElement && jsonElement.ValueKind == JsonValueKind.Array)
        {
            var sensors = JsonSerializer.Deserialize<int?[]>(jsonElement.GetRawText());
            if (sensors != null && sensors.Length > 0)
            {
                flight.Sensors.AddRange(sensors);
            }
        }

        return flight;
    }

    private static float? ParseFloat(object? value) =>
        float.TryParse(value?.ToString(), out float result) ? result : null;

    private static int? ParseInt(object? value) =>
        int.TryParse(value?.ToString(), out int result) ? result : null;
}