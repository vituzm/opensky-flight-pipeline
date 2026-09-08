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
        _logger.LogInformation("Press S to consume messages, Q to stop.");

        _consumer.Subscribe(TopicName);
        
        while (!stoppingToken.IsCancellationRequested)
        {
            var key = await Task.Run(() => Console.ReadKey(intercept: true), stoppingToken);

            if (key.Key == ConsoleKey.S) {
                var result = _consumer.Consume(stoppingToken);
                await HandleMessage(result, stoppingToken);
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
