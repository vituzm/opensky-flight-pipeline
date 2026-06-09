namespace IngestorOpenSky.Interfaces;

public interface IOpenSkyClient
{
    Task<OpenSkyResponse> GetDataOpenSky(Dictionary<string, string?> parametros);
}