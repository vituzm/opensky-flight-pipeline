namespace IngestorOpenSky.Services;

using Confluent.Kafka;
using IngestorOpenSky.Interfaces;
using IngestorOpenSky.Models;

public class KafkaProducerService : IKafkaProducerService, IDisposable
{
    private readonly IProducer<string, string> _producer;
    private readonly ILogger<KafkaProducerService> _logger;
    private readonly ProducerConfig _config = new ProducerConfig
    {
        BootstrapServers = "localhost:9092,localhost:9094,localhost:9095",
    };
    
    public KafkaProducerService(ILogger<KafkaProducerService> logger)
    {
        _logger = logger;
        _producer = new ProducerBuilder<string, string>(_config).Build();
    }

    public void SendMessages(List<KafkaEvent> kafkaEvents, string topicName)
    {
        foreach (var eventKafka in kafkaEvents)
        {
            var message = KafkaEventToMessage(eventKafka);
            SendMessageAsync(message, topicName);
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

    private void SendMessageAsync(Message<string, string> message, string topicName)
    {

        _producer.Produce(topicName, message, (deliveryHandler) => {
            if(deliveryHandler.Error.IsError)
            {
                _logger.LogError($"Error sending message to Kafka: {deliveryHandler.Error.Reason}");
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
            _logger.LogCritical($"Flush timed out. {remainingQueueCount} messages left.");
            // Rocks DB persistence
        };

        _producer.Dispose();
    }
}