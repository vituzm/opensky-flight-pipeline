namespace ProducerOpenSky.Services;

using System.Text.Json;
using ProducerOpenSky.Interfaces;
using ProducerOpenSky.Models;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Options;

public class OpenSkyClient : IOpenSkyClient
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<OpenSkyClient> _logger;
    private readonly OpenSkyOptions _config;
    private static readonly JsonSerializerOptions _jsonOptions = new() { PropertyNameCaseInsensitive = true};
    private readonly IHostEnvironment _env;


    public OpenSkyClient(
        ILogger<OpenSkyClient> logger, 
        IHttpClientFactory httpClientFactory, 
        IHostEnvironment env,
        IOptions<OpenSkyOptions> openSkyOptions)
    {
        _config = openSkyOptions.Value;
        _logger = logger;
        _httpClientFactory = httpClientFactory;
        _env = env;
    }

    public async Task<OpenSkyResponse> GetDataOpenSky(Dictionary<string, string?> parameters)
    {
        var client = _httpClientFactory.CreateClient();
        var endpoint = BuildEndpoint(parameters);
        
        int maxAttempts = _config.MaxRetries;
        TimeSpan delay = TimeSpan.FromSeconds(5); // Começa esperando 5 segundos

        for (int i = 1; i <= maxAttempts; i++)
        {
            try
            {
                using var response = await client.GetAsync(endpoint, HttpCompletionOption.ResponseHeadersRead);
                
                ValidateReponse(response);

                var apiResponse = await ProcessResponse(response);

                LogDetalhesDev(parameters, response, apiResponse);
                
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
        var uri = QueryHelpers.AddQueryString(_config.OpenSkyUrl, parameters);
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

        return apiResponse;
    }

    private void LogDetalhesDev(Dictionary<string, string?> parameters, HttpResponseMessage response, OpenSkyResponse apiResponse)
    {
        if (!_env.IsDevelopment()) return;

        int count = apiResponse?.StatesRaw?.Count ?? 0;
        int time = apiResponse?.Time ?? 0;

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