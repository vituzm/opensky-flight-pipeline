namespace ProducerOpenSky.Interfaces;

using ProducerOpenSky.Models;

public interface IOpenSkyClient
{
    Task<OpenSkyResponse> GetDataOpenSky(Dictionary<string, string?> parametros);
}