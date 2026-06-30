namespace IngestorOpenSky.Services;

using System.Text.Json;
using IngestorOpenSky.Interfaces;
using IngestorOpenSky.Models;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Options;

public class OpenSkyClient : IOpenSkyClient
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<OpenSkyClient> _logger;
    private readonly string _openSkyUrl;
    private static readonly JsonSerializerOptions _jsonOptions = new() { PropertyNameCaseInsensitive = true};
    private readonly IHostEnvironment _env;
    private readonly int _maxRetries;


    public OpenSkyClient(
        ILogger<OpenSkyClient> logger, 
        IHttpClientFactory httpClientFactory, 
        IHostEnvironment env,
        IOptions<OpenSkyOptions> openSkyOptions)
    {
        _openSkyUrl = openSkyOptions.Value.OpenSkyUrl;
        _logger = logger;
        _httpClientFactory = httpClientFactory;
        _env = env;
        _maxRetries = openSkyOptions.Value.MaxRetries;
    }

    public async Task<OpenSkyResponse> GetDataOpenSky(Dictionary<string, string?> parameters)
    {
        var client = _httpClientFactory.CreateClient();
        var endpoint = BuildEndpoint(parameters);
        
        int maxAttempts = _maxRetries;
        TimeSpan delay = TimeSpan.FromSeconds(5); // Começa esperando 5 segundos

        for (int i = 1; i <= maxAttempts; i++)
        {
            try
            {
                using var response = await client.GetAsync(endpoint, HttpCompletionOption.ResponseHeadersRead);
                
                ValidateReponse(response);

                var apiResponse = await ProcessResponse(response);
                
                LogDetalhesDev(parameters, response, apiResponse.States.Count, apiResponse.Time);
                
                return apiResponse;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogWarning("Attempt {i} has failed: {msg}", i, ex.Message);

                if (i == maxAttempts)
                {
                    _logger.LogError("Maximum number of attempts reached.");
                    throw;
                }

                await Task.Delay(delay);
                delay *= 2;
            }
        }

        return new OpenSkyResponse();
    }

    private Uri BuildEndpoint(Dictionary<string, string?> parameters)
    {
        var uri = QueryHelpers.AddQueryString(_openSkyUrl, parameters);
        return new Uri(uri);
    }

    private void ValidateReponse(HttpResponseMessage response)
    {   
        if(response.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
        {
            _logger.LogWarning("Maximum number of requests reached (429). Check the X-Rate-Limit-Remaining header for rate limit details.");
        }
        else if (!response.IsSuccessStatusCode)
        {
            _logger.LogError("OpenSky API returned an error: {statusCode}", response.StatusCode);
            throw new HttpRequestException($"OpenSky Error: {response.StatusCode}");
        }
    }

    private async Task<OpenSkyResponse> ProcessResponse(HttpResponseMessage response)
    {
        using var contentStream = await response.Content.ReadAsStreamAsync();
        var apiResponse = await JsonSerializer.DeserializeAsync<OpenSkyResponse>(contentStream, _jsonOptions);

        if (apiResponse?.StatesRaw == null)
        {
            _logger.LogWarning("OpenSky API returned an empty response or no flight states.");
            return apiResponse ?? new OpenSkyResponse();
        }

        // Mapeamento
        apiResponse.States = apiResponse.StatesRaw
            .Select(FlightState.MapArrayFlightState)
            .ToList();

        return apiResponse;
    }

    private void LogDetalhesDev(Dictionary<string, string?> parameters, HttpResponseMessage response, int count, int time)
    {
        if (!_env.IsDevelopment()) return;

        // Formata os parâmetros do dicionário
        var paramsFormatados = string.Join("\n    ", parameters.Select(p => $"{p.Key}: {p.Value ?? "null"}"));

        // Formata os Headers (importante para ver o Rate Limit da OpenSky)
        var headersFormatados = string.Join("\n    ", response.Headers.Select(h => $"{h.Key}: {string.Join(", ", h.Value)}"));

        _logger.LogInformation("""
            === RELATÓRIO DE INGESTÃO (DEV MODE) ===
            
            [PARÂMETROS DA REQUISIÇÃO]
                {params}
                
            [RESUMO DO PROCESSAMENTO]
                Voos Processados: {count}
                Timestamp API: {time}
                Status Code: {status}

            [HEADERS DA RESPOSTA]
                {headers}
                
            ========================================
            """, 
            paramsFormatados, 
            count, 
            time, 
            response.StatusCode, 
            headersFormatados);
    }
}