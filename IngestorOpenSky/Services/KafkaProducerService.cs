namespace IngestorOpenSky.Services;

using System.Text;
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

    public void EnviarMensagem(OpenSkyResponse response, Dictionary<string, string?> parametros)
    {
        int unixTimestamp = response.Time;
        List<FlightState> voos = response.States;
        
        Headers HeadersApiRequest = BuildHeaders(unixTimestamp, parametros);

        foreach(var voo in voos)
        {
            string jsonValue = System.Text.Json.JsonSerializer.Serialize(voo);

            var mensagem = new Message<string, string>
            {
                Key = voo.Icao24, // Identificador único do avião
                Value = jsonValue
            };
            // a chave do kafka nao deveria ser o icao24 + timestamp?
            // todo: logica de retry, dead letter queue com o rocks db 
        }
    }

    private Headers BuildHeaders(int unixTimestamp,Dictionary<string, string?> parametros)
    {
        var HeadersApiRequest = new Headers 
        { 
                { "api_unix_time", Encoding.UTF8.GetBytes(unixTimestamp.ToString()) },
                { "source", Encoding.UTF8.GetBytes("opensky_api") },
        };

        foreach (var param in parametros)
        {
            if (param.Value != null)
            {
                HeadersApiRequest.Add(param.Key, Encoding.UTF8.GetBytes(param.Value));
            }
        }

        return HeadersApiRequest;
    }  
}