namespace IngestorOpenSky.Services;

using System.Text.Json;
using IngestorOpenSky.Interfaces;
using IngestorOpenSky.Models;
using Microsoft.AspNetCore.WebUtilities;

public class OpenSkyClient : IOpenSkyClient
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<OpenSkyClient> _logger;

    public OpenSkyClient(ILogger<OpenSkyClient> logger, IHttpClientFactory httpClientFactory)
    {
        _logger = logger;
        _httpClientFactory = httpClientFactory;
    }

    public async Task<List<OpenSkyResponse>> GetDadosOpenSky(Dictionary<string, string?> parametros)
    {
        HttpClient openSkyClient = _httpClientFactory.CreateClient();
        Uri endpoint = ConstruirEndpoint(parametros);

        var response = await openSkyClient.GetAsync(endpoint);
        await ValidarResponse(response);

        var jsonString = await response.Content.ReadAsStringAsync();
        var apiResponse = JsonSerializer.Deserialize<OpenSkyResponse>(jsonString, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        var listaVoos = apiResponse.States.Select(elemento => FlightState.MapearDoArray(elemento)).ToList();

        apiResponse.
    }

    public async Task ValidarResponse(HttpResponseMessage response)
    {
        
        if (response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync();
            _logger.LogInformation("Headers dados OpenSky: {response.Headers}", response.Headers);
            _logger.LogInformation("Dados OpenSky: {content}", content);
        }
        else
        {
            _logger.LogError("Erro ao obter dados do OpenSky: {statusCode}", response.StatusCode);
        }
    }

    public Uri ConstruirEndpoint(Dictionary<string, string?> parametros)
    {
        var endpoint = new Uri(QueryHelpers.AddQueryString("https://opensky-network.org/api/states/all", parametros));
        _logger.LogInformation("Link endpoint: {endpoint}", endpoint);

        return endpoint;    

    }
}