namespace IngestorOpenSky.Services;

using System.Text.Json;
using Confluent.Kafka;
using IngestorOpenSky.Interfaces;
using IngestorOpenSky.Models;

public class KafkaProducerService : IKafkaProducerService, IDisposable
{
    private readonly IProducer<string, string> _producer;
    private readonly ProducerConfig _config;
    private readonly ILogger<KafkaProducerService> _logger;
    private readonly IEventFailureRepository _eventFailureRepository;
    
    public KafkaProducerService(ILogger<KafkaProducerService> logger, IEventFailureRepository eventFailureRepository)
    {
        _logger = logger;
        _eventFailureRepository = eventFailureRepository;
        _config = new ProducerConfig
            {
                BootstrapServers = "localhost:9092,localhost:9094,localhost:9095",
                MessageTimeoutMs = 5000
            };

        _producer = new ProducerBuilder<string, string>(_config)
            .SetLogHandler((_, logMessage) => 
            {
                
            })
            .Build();
    }

    public void SendMessages(List<KafkaEvent> kafkaEvents)
    {
        foreach (var eventKafka in kafkaEvents)
        {
            SendMessageAsync(eventKafka);
        }

    }

    private Message<string, string> KafkaEventToMessage(KafkaEvent eventoKafka)
    {
        var message = new Message<string, string>
        {
            Key = eventoKafka.Key,
            Value = eventoKafka.Value,
            Headers = KafkaHeader(eventoKafka.Headers)
        };

        return message;
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

    private void SendMessageAsync(KafkaEvent eventKafka)
    {
        var message = KafkaEventToMessage(eventKafka);

        _producer.Produce(eventKafka.Topic, message, (deliveryHandler) => {
            if(deliveryHandler.Error.IsError)
            {
                _logger.LogError($"Error sending message to Kafka: {deliveryHandler.Error.Reason}");

                string rocksKey = $"{eventKafka.Topic}_{eventKafka.Key}_{DateTime.UtcNow.Ticks}";
                
                _eventFailureRepository.SaveMessageFailure(rocksKey, JsonSerializer.Serialize(eventKafka));
            }
            else
            {
                _logger.LogInformation($"Message sent successfully to Kafka: {deliveryHandler.TopicPartitionOffset}");
            
            };
        });
    }

    public void Dispose()
    {
        _logger.LogInformation("Disposing Kafka Producer...");
        int remainingQueueCount = _producer.Flush(TimeSpan.FromSeconds(60));

        if (remainingQueueCount > 0)
        {
            _logger.LogInformation($"Flush timed out. {remainingQueueCount} messages left.");
            _logger.LogInformation($"Messages failed were persisted.");
        };
        _producer.Dispose();
    }
}