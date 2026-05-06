
using IngestorOpenSky.Interfaces;
using IngestorOpenSky.Models;
using IngestorOpenSky.Services;

namespace IngestorOpenSky;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly IOpenSkyClient _openSkyClient;
    private readonly KafkaProducerService _kafkaProducerService; // todo: criar uma interface para o producer

    public Worker(ILogger<Worker> logger, IOpenSkyClient openSkyClient, KafkaProducerService kafkaProducerService)
    {
        _logger = logger;
        _openSkyClient = openSkyClient;
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

                OpenSkyResponse response = await _openSkyClient.GetDadosOpenSky(dict_parametros);
                _kafkaProducerService.EnviarMensagem(response, dict_parametros);
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