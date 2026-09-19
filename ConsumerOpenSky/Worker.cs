using Confluent.Kafka;
using Confluent.SchemaRegistry;
using Confluent.SchemaRegistry.Serdes;
using ConsumerOpenSky.Models;

namespace ConsumerOpenSky;

public class Worker : BackgroundService
{
    private const string TopicName = "flight-data";
    private readonly IConsumer<string, byte[]> _consumer;
    private ProtobufDeserializer<FlightState> _protobufDeserializer;
    private readonly ILogger<Worker> _logger;
    private int _messages_read = 1;

    public Worker(IConsumer<string, byte[]> consumer, ISchemaRegistryClient schemaRegistry, ILogger<Worker> logger)
    {
        _consumer = consumer;
        _protobufDeserializer = new(schemaRegistry);
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Consumer initialization.");

        _consumer.Subscribe(TopicName);
        
        while (!stoppingToken.IsCancellationRequested)
        {
            _logger.LogInformation("Press S to consume messages, Q to stop.");

            var key = await Task.Run(() => Console.ReadKey(intercept: true), stoppingToken);

            if (key.Key == ConsoleKey.S) {
                try
                {
                    _logger.LogInformation("Consuming message...");
                    var result = _consumer.Consume(TimeSpan.FromSeconds(20)); // Seconds to wait for a message
                    if (result == null)
                    {
                        _logger.LogInformation("No message received.");
                        continue;
                    }

                    await HandleMessage(result, stoppingToken);
                }
                catch (ConsumeException ex)
                {
                    if (ex.Error.Code == ErrorCode.UnknownTopicOrPart)
                    {
                        _logger.LogInformation($"Error: The subscribed topic is not available. Details: {ex.Error.Reason}");
                        _logger.LogInformation("Please check the topic and try again.");
                        _consumer.Subscribe(TopicName);
                    }
                    else
                    {
                        _logger.LogError($"Other Kafka consume error: {ex.Error.Reason}");
                        throw;
                    }
                }
            }
            else if (key.Key == ConsoleKey.Q)
            {
                _logger.LogInformation("Shutting down worker...");
                break;
            }
        }

        _consumer.Close();
    }


    protected virtual async Task HandleMessage(ConsumeResult<string, byte[]> result, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Mensagem recebida");

        var context = new SerializationContext(MessageComponentType.Value, result.Topic);
        
        FlightState flight = await _protobufDeserializer.DeserializeAsync(
            result.Message.Value, 
            isNull: false, 
            context
        );

        _logger.LogInformation("Mensagem recebida - ICAO24: {Icao24}, País: {OriginCountry}", 
            flight.Icao24, 
            flight.OriginCountry);

        _logger.LogInformation("Mensagens recebidas - {_messages_read}", _messages_read);

        _messages_read++;
    }
}
