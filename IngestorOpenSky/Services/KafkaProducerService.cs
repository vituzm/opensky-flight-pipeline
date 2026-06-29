namespace IngestorOpenSky.Services;

using Confluent.Kafka;
using IngestorOpenSky.Interfaces;
using IngestorOpenSky.Models;
using IngestorOpenSky.Models.Config;
using Microsoft.Extensions.Options;

public class KafkaProducerService : IKafkaProducerService, IDisposable
{
    private readonly IProducer<string, string> _producer;
    private readonly ProducerConfig _config;
    private readonly ILogger<KafkaProducerService> _logger;
    
    public KafkaProducerService(
        ILogger<KafkaProducerService> logger, 
        IOptions<KafkaOptions> kafkaOptions)
    {
        _logger = logger;
        _config = new ProducerConfig
            {
                BootstrapServers = kafkaOptions.Value.BootstrapServers,
                MessageTimeoutMs = kafkaOptions.Value.MessageTimeoutMs
            };

        _producer = new ProducerBuilder<string, string>(_config)
            .SetLogHandler((_, logMessage) => 
            {
                
            })
            .Build();
    }

    public void SendMessages(
        List<KafkaEvent> kafkaEvents,
        Action<KafkaEvent> onSuccess,
        Action<KafkaEvent> onError
    )
    {
        foreach (var eventKafka in kafkaEvents)
        {
            SendMessage(eventKafka, onSuccess, onError);
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

    public void SendMessage(
        KafkaEvent eventKafka, 
        Action<KafkaEvent> onSuccess, 
        Action<KafkaEvent> onError)
    {
        var message = KafkaEventToMessage(eventKafka);

        _producer.Produce(eventKafka.Topic, message, (deliveryHandler) => {
            if(deliveryHandler.Error.IsError) onError(eventKafka);
            else onSuccess(eventKafka);
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