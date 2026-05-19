namespace IngestorOpenSky.Services;

using Confluent.Kafka;
using IngestorOpenSky.Interfaces;
using IngestorOpenSky.Models;

public class KafkaProducerService : IKafkaProducerService
{
    private readonly IProducer<string, string> _producer; // Temporariamente string (JSON)
    private readonly ILogger<KafkaProducerService> _logger;
    private readonly ProducerConfig _config = new ProducerConfig
    {
        BootstrapServers = "localhost:9092,localhost:9094,localhost:9095"
    };
    
    public KafkaProducerService(ILogger<KafkaProducerService> logger)
    {
        _logger = logger;
        _producer = new ProducerBuilder<string, string>(_config).Build();
    }

    public void EnviarMensagensOpenSky(List<KafkaEvent> kafkaEvents, string topicName)
    {
        foreach (var eventKafka in kafkaEvents)
        {
            var mensagem = KafkaEventToMessage(eventKafka);
            EnviarMensagemAsync(mensagem, topicName);
        }

        _producer.Flush();
    }

    private Message<string, string> KafkaEventToMessage(KafkaEvent eventoKafka)
    {
        var mensagem = new Message<string, string>
        {
            Key = eventoKafka.Key,
            Value = eventoKafka.Value,
            Headers = KafkaHeader(eventoKafka.Headers)
        };

        return mensagem;
    }

    private Headers KafkaHeader(Dictionary<string, byte[]> headers)
    {
        var kafkaHeaders = new Headers();
        foreach (var header in headers)
        {
            kafkaHeaders.Add(header.Key, header.Value);
        }
        return kafkaHeaders;
    }

    private void EnviarMensagemAsync(Message<string, string> mensagem, string topicName)
    {

        _producer.Produce(topicName, mensagem, (deliveryHandler) => {
            if(deliveryHandler.Error.IsError)
            {
                _logger.LogError($"Erro ao enviar mensagem para Kafka: {deliveryHandler.Error.Reason}");
            }
            else
            {
                _logger.LogInformation($"Mensagem enviada com sucesso para Kafka: {deliveryHandler.TopicPartitionOffset}");
            
            };
        });

    }
}