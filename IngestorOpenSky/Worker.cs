
using IngestorOpenSky.Interfaces;
using IngestorOpenSky.Models;

namespace IngestorOpenSky;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly IOpenSkyClient _openSkyClient;

    public Worker(ILogger<Worker> logger, IOpenSkyClient openSkyClient)
    {
        _logger = logger;
        _openSkyClient = openSkyClient;
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
                    {"lamin", "3.339844"},
                    {"lomin", "12.382928"},
                    {"lamax", "15.644531"},
                    {"lomax", "23.483401"},
                    {"extended", "1"}
                };

                await _openSkyClient.GetDadosOpenSky(dict_parametros);
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