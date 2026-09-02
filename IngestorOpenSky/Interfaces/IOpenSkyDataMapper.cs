namespace IngestorOpenSky.Interfaces;
using IngestorOpenSky.Models;
public interface IOpenSkyDataMapper
{
    Task<List<KafkaEvent>> MapToKafkaEvents(OpenSkyResponse response, Dictionary<string, string?> parameters, string topic);
}