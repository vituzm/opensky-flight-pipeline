
using IngestorOpenSky.Interfaces;
using IngestorOpenSky.Models;

namespace IngestorOpenSky;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly IOpenSkyClient _openSkyClient;
    private readonly IOpenSkyDataMapper _flightDataMapper;
    private readonly IKafkaProducerService _kafkaProducerService;
    private readonly IEventFailureRepository _eventFailureRepository;

    public Worker(
        ILogger<Worker> logger, 
        IOpenSkyClient openSkyClient, 
        IOpenSkyDataMapper flightDataMapper, 
        IKafkaProducerService kafkaProducerService,
        IEventFailureRepository eventFailureRepository
    )
    {
        _logger = logger;
        _openSkyClient = openSkyClient;
        _flightDataMapper = flightDataMapper;
        _kafkaProducerService = kafkaProducerService;
        _eventFailureRepository = eventFailureRepository;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Worker waitint for command. Press 's' to initiate, 'q' to exit. Press 'r' to reproce failed messages.");

        while (!stoppingToken.IsCancellationRequested)
        {
            var key = Console.ReadKey(intercept: true);
            if (key.Key == ConsoleKey.S)
            {
                _logger.LogInformation("Initializing data ingestion...");

                var dict_parametros = new Dictionary<string, string?>
                {
                    {"time", null},
                    {"icao24", null},
                    {"lamin", "-35.546753"},
                    {"lomin", "-61.369629"},
                    {"lamax", "-26.013595"},
                    {"lomax", "-47.746582"},
                    {"extended", "1"}
                };

                OpenSkyResponse response = await _openSkyClient.GetDataOpenSky(dict_parametros);
                List<KafkaEvent> kafkaEvents = _flightDataMapper.MapToKafkaEvents(response, dict_parametros, "flight-data");
                                
                _kafkaProducerService.SendMessages(kafkaEvents);

                _logger.LogInformation("Data ingestion completed. Press 's' for new request or 'q' to quit.");
            }
            else if (key.Key == ConsoleKey.Q)
            {
                _logger.LogInformation("Shutting down worker...");
                break;
            }
            else if (key.Key == ConsoleKey.R)
            {
                _logger.LogInformation("Reprocessing failed messages...");
                var falhas = _eventFailureRepository.GetAllMessageFailures();

                if (falhas.Count == 0)
                {
                    _logger.LogWarning("RocksDB is empty!");
                }
                else
                {
                    _logger.LogInformation($"Failed messages found: {falhas.Count}");
                    foreach (var item in falhas)
                    {
                        Console.WriteLine($"\n[KEY]: {item.Key}");
                        Console.WriteLine($"[VALUE]: {item.Value}");
                        Console.WriteLine(new string('-', 50));
                    }
                }
                _logger.LogInformation("End of query. 's' for Ingestion, 'r' for Reload, 'q' for Exit.");
            }
        }
    }
    
}