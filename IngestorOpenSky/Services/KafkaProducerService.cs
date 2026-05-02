namespace IngestorOpenSky.Services;

using Confluent.Kafka;
using IngestorOpenSky.Models;

public class KafkaProducerService
{
    private readonly IProducer<string, string> _producer; // Temporariamente string (JSON)
    private readonly ILogger<KafkaProducerService> _logger;
    private const string TopicName = "flight-data";

    public KafkaProducerService(ILogger<KafkaProducerService> logger)
    {
        _logger = logger;
        var config = new ProducerConfig { 
            BootstrapServers = "localhost:9092,localhost:9094,localhost:9095"
        };

        _producer = new ProducerBuilder<string, string>(config).Build();
    }

    public async Task EnviarMensagem(OpenSkyResponse response)
    {
        int unixTimestamp = response.Time;
        List<FlightState> voos = response.States;
        
        foreach(voo in voos)
        {
            string jsonValue = System.Text.Json.JsonSerializer.Serialize(voo);

            var Headers = new Headers 
            { 
                { "api_unix_time", Encoding.UTF8.GetBytes(voo.Time.ToString()) },
                { "source", Encoding.UTF8.GetBytes("opensky_api") }
            };
            
            var mensagem = new Message<string, string>
            {
                Key = voo.Icao24, // Identificador único do avião
                Value = jsonValue
            };

        }
    }
}