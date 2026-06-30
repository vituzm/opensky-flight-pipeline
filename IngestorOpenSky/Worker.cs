
using System.Text.Json;
using IngestorOpenSky.Interfaces;
using IngestorOpenSky.Models;
using Microsoft.Extensions.Options;

namespace IngestorOpenSky;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly IOpenSkyClient _openSkyClient;
    private readonly IOpenSkyDataMapper _flightDataMapper;
    private readonly IKafkaProducerService _kafkaProducerService;
    private readonly IEventFailureRepository _eventFailureRepository;
    private readonly IOptions<OpenSkyOptions> _openSkyOptions;

    public Worker(
        ILogger<Worker> logger, 
        IOpenSkyClient openSkyClient, 
        IOpenSkyDataMapper flightDataMapper, 
        IKafkaProducerService kafkaProducerService,
        IEventFailureRepository eventFailureRepository,
        IOptions<OpenSkyOptions> openSkyOptions
    )
    {
        _logger = logger;
        _openSkyClient = openSkyClient;
        _flightDataMapper = flightDataMapper;
        _kafkaProducerService = kafkaProducerService;
        _eventFailureRepository = eventFailureRepository;
        _openSkyOptions = openSkyOptions;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Worker waitint for command. Press 's' to initiate, 'q' to exit. Press 'r' to reproce failed messages.");

        while (!stoppingToken.IsCancellationRequested)
        {
            var key = await Task.Run(() => Console.ReadKey(intercept: true), stoppingToken);

            if (key.Key == ConsoleKey.S)
            {
                _logger.LogInformation("Initializing data ingestion...");

                var boundingBox = _openSkyOptions.Value.BoundingBox;
                var dict_parametros = new Dictionary<string, string?>
                {
                    {"time", null},
                    {"icao24", null},
                    {"lamin", boundingBox.Lamin},
                    {"lomin", boundingBox.Lomin},
                    {"lamax", boundingBox.Lamax},
                    {"lomax", boundingBox.Lomax},
                    {"extended", "1"}
                };

                OpenSkyResponse response = await _openSkyClient.GetDataOpenSky(dict_parametros);
                List<KafkaEvent> kafkaEvents = _flightDataMapper.MapToKafkaEvents(response, dict_parametros, "flight-data");
                
                var onSuccess = DeliveryHandlers.NoOp;
                var onError   = (KafkaEvent e) => 
                    {
                        _eventFailureRepository.SaveMessageFailure($"{e.Topic}_{e.Key}_{DateTime.UtcNow.Ticks}", JsonSerializer.Serialize(e));
                        _logger.LogError($"Failed to send message with key {e.Key} to topic {e.Topic}. Message saved for reprocessing.");
                    };

                _kafkaProducerService.SendMessages(kafkaEvents, onSuccess, onError);

                _logger.LogInformation("Data ingestion completed. Press 's' for new request, 'r' to reprocess failed messages, or 'q' to quit.");
            }
            else if (key.Key == ConsoleKey.R)
            {
                
                var messageFailures = _eventFailureRepository.GetAllMessageFailures();

                _logger.LogInformation($"Reprocessing failed messages: {messageFailures.Count}...");

                if (messageFailures.Count == 0)
                {
                    _logger.LogWarning("RocksDB is empty!");
                    continue;
                }

                foreach (var (rocksKey, eventJson) in messageFailures)
                {
                    var onSuccess = (KafkaEvent e) => {
                        _eventFailureRepository.RemoveMessage(rocksKey);
                    };

                    var onError   = (KafkaEvent e) => 
                    {
                        _logger.LogError($"""
                            Failed to send message with key {e.Key} to topic {e.Topic}. Message remains in RocksDB for reprocessing.
                            Press 's' for new request, 'r' to reprocess failed messages, or 'q' to quit.
                        """);
                    };

                    var kafkaEvent = new KafkaEvent();

                    try
                    {
                        kafkaEvent = JsonSerializer.Deserialize<KafkaEvent>(eventJson);
                        if (kafkaEvent == null)
                        {
                            _logger.LogWarning($"Message with key {rocksKey} deserialized to null.");
                            _eventFailureRepository.RemoveMessage(rocksKey);
                            continue;
                        }
                    }
                    catch (JsonException ex)
                    {
                        _logger.LogError($"Failed to deserialize message with key {rocksKey}: {ex.Message}");
                        _eventFailureRepository.RemoveMessage(rocksKey);
                        continue;
                    }

                    _kafkaProducerService.SendMessage(kafkaEvent, onSuccess, onError);
                }
                

                _logger.LogInformation("End of query. 's' for Ingestion, 'r' for Reload, 'q' for Exit.");
            }
            else if (key.Key == ConsoleKey.Q)
            {
                _logger.LogInformation("Shutting down worker...");
                break;
            }
        }
    }
    
}