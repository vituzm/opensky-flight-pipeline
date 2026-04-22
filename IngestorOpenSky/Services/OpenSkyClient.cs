namespace IngestorOpenSky.Services;

using System.Text.Json;
using IngestorOpenSky.Interfaces;
using IngestorOpenSky.Models;
using Microsoft.AspNetCore.WebUtilities;

public class OpenSkyClient : IOpenSkyClient
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<OpenSkyClient> _logger;
    
    private const string OpenSkyUrl = "https://opensky-network.org/api/states/all";
    
    private static readonly JsonSerializerOptions _jsonOptions = new() { PropertyNameCaseInsensitive = true};
    private readonly IHostEnvironment _env;

    public OpenSkyClient(ILogger<OpenSkyClient> logger, IHttpClientFactory httpClientFactory, IHostEnvironment env)
    {
        _logger = logger;
        _httpClientFactory = httpClientFactory;
        _env = env;
    }

    public async Task<OpenSkyResponse> GetDadosOpenSky(Dictionary<string, string?> parametros)
    {
        var client = _httpClientFactory.CreateClient();
        var endpoint = ConstruirEndpoint(parametros);
        
        int maxTentativas = 3;
        TimeSpan delay = TimeSpan.FromSeconds(5); // Começa esperando 5 segundos

        for (int i = 1; i <= maxTentativas; i++)
        {
            try
            {
                using var response = await client.GetAsync(endpoint, HttpCompletionOption.ResponseHeadersRead);
                
                ValidarResponse(response);

                var apiResponse = await ProcessarResposta(response);
                LogDetalhesDev(parametros, response, apiResponse.States.Count, apiResponse.Time);
                
                return apiResponse;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogWarning("Tentativa {i} falhou: {msg}", i, ex.Message);

                if (i == maxTentativas)
                {
                    _logger.LogError("Número máximo de tentativas atingido.");
                    throw;
                }

                await Task.Delay(delay);
                delay *= 2;
            }
        }

        return new OpenSkyResponse();
    }

    private Uri ConstruirEndpoint(Dictionary<string, string?> parametros)
    {
        var uri = QueryHelpers.AddQueryString(OpenSkyUrl, parametros);
        return new Uri(uri);
    }

    private void ValidarResponse(HttpResponseMessage response)
    {   
        if(response.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
        {
            _logger.LogWarning("Limite de requisições atingido (429). Verifique os header X-Rate-Limit-Remaining para detalhes do rate limit.");
        }
        else if (!response.IsSuccessStatusCode)
        {
            _logger.LogError("API OpenSky retornou erro: {statusCode}", response.StatusCode);
            throw new HttpRequestException($"Erro OpenSky: {response.StatusCode}");
        }
    }

    private async Task<OpenSkyResponse> ProcessarResposta(HttpResponseMessage response)
    {
        using var contentStream = await response.Content.ReadAsStreamAsync();
        var apiResponse = await JsonSerializer.DeserializeAsync<OpenSkyResponse>(contentStream, _jsonOptions);

        if (apiResponse?.StatesRaw == null)
        {
            _logger.LogWarning("Resposta vazia ou sem estados de voo.");
            return apiResponse ?? new OpenSkyResponse();
        }

        // Mapeamento
        apiResponse.States = apiResponse.StatesRaw
            .Select(FlightState.MapearDoArray)
            .ToList();

        return apiResponse;
    }

    private void LogDetalhesDev(Dictionary<string, string?> parametros, HttpResponseMessage response, int count, int time)
    {
        if (!_env.IsDevelopment()) return;

        // Formata os parâmetros do dicionário
        var paramsFormatados = string.Join("\n    ", parametros.Select(p => $"{p.Key}: {p.Value ?? "null"}"));

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