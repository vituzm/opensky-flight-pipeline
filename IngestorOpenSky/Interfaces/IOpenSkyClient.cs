namespace IngestorOpenSky.Interfaces;

public interface IOpenSkyClient
{
    Task<OpenSkyResponse> GetDadosOpenSky(Dictionary<string, string?> parametros);
}