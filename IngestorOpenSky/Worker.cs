
using IngestorOpenSky.Interfaces;
using IngestorOpenSky.Models;

namespace IngestorOpenSky;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly IOpenSkyClient _openSkyClient;
    private readonly IFlightDataMapper _flightDataMapper;

    private readonly IKafkaProducerService _kafkaProducerService;

    public Worker(ILogger<Worker> logger, IOpenSkyClient openSkyClient, IFlightDataMapper flightDataMapper, IKafkaProducerService kafkaProducerService)
    {
        _logger = logger;
        _openSkyClient = openSkyClient;
        _flightDataMapper = flightDataMapper;
        _kafkaProducerService = kafkaProducerService;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Worker aguardando comando. Pressione 's' para iniciar ingestão, 'q' para sair.");

        while (!stoppingToken.IsCancellationRequested)
        {
            var key = Console.ReadKey(intercept: true);
            if (key.Key == ConsoleKey.S)
            {
                _logger.LogInformation("Iniciando ingestão de dados...");

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
                List<KafkaEvent> kafkaEvents = _flightDataMapper.MapToKafkaEvents(response, dict_parametros);
                Console.WriteLine($"Total de eventos mapeados: {kafkaEvents.Count}");
                foreach (var evento in kafkaEvents.Take(5)) // Exibe os primeiros 5 eventos para verificação
                {
                    Console.WriteLine($"Evento: Key={evento.Key}, Value={evento.Value}, Headers={string.Join(", ", evento.Headers.Select(h => $"{h.Key}={Convert.ToBase64String(h.Value)}"))}");
                }
                
                // _kafkaProducerService.SendMessages(kafkaEvents, "flight-data");

                _logger.LogInformation("Ingestão concluída. Pressione 's' para nova requisição ou 'q' para sair.");
            }
            else if (key.Key == ConsoleKey.Q)
            {
                _logger.LogInformation("Encerrando worker...");
                break;
            }
        }
    }
    
}