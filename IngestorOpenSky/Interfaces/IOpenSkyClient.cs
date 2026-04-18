using IngestorOpenSky.Models;

namespace IngestorOpenSky.Interfaces;

public interface IOpenSkyClient
{
    Task<List<OpenSkyResponse>> GetDadosOpenSky(Dictionary<string, string?> parametros);
    Uri ConstruirEndpoint(Dictionary<string, string?> parametros);
    Task ValidarResponse(HttpResponseMessage response);
}