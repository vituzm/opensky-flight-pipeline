namespace IngestorOpenSky.Services;

using System.Text;
using Confluent.Kafka;
using IngestorOpenSky.Interfaces;
using IngestorOpenSky.Models;

public class KafkaProducerService : IKafkaProducerService
{
    private readonly IProducer<string, string> _producer; // Temporariamente string (JSON)
    private readonly ILogger<KafkaProducerService> _logger;
    private const string TopicName = "flight-data";
    private readonly ProducerConfig _config = new ProducerConfig
    {
        BootstrapServers = "localhost:9092,localhost:9094,localhost:9095"
    };
    
    public KafkaProducerService(ILogger<KafkaProducerService> logger)
    {
        _logger = logger;
        _producer = new ProducerBuilder<string, string>(_config).Build();
    }

    public void EnviarMensagensOpenSky(OpenSkyResponse response, Dictionary<string, string?> parametros)
    {
        string unixTimestamp = response.Time.ToString();
        Headers headers = BuildHeaders(unixTimestamp, parametros);

        foreach (var voo in response.States)
        {
            EnviarMensagemAsync(headers, voo);
        }

        _producer.Flush();
    }

    private static Headers BuildHeaders(string unixTimestamp, Dictionary<string, string?> parametros)
    {
        var headers = new Headers
        {
            { "api_unix_time", Encoding.UTF8.GetBytes(unixTimestamp) },
            { "source", Encoding.UTF8.GetBytes("opensky_api") }
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

    private void EnviarMensagemAsync(Headers headers, FlightState voo)
    {
        string jsonValue = System.Text.Json.JsonSerializer.Serialize(voo);

        var mensagem = new Message<string, string>
        {
            Key = voo.Icao24, // Identificador único do avião
            Value = jsonValue
        };

        _producer.Produce(topic, mensagem, (deliveryHandler) => {
            if(deliveryHandler.Error.IsError)
            {
                _logger.LogError($"Erro ao enviar mensagem para Kafka: {deliveryHandler.Error.Reason}");
            }
            else
            {
                _logger.LogInformation($"Mensagem enviada com sucesso para Kafka: {deliveryHandler.TopicPartitionOffset}");
            
            }
        })

    }
}