namespace IngestorOpenSky.Interfaces;
using IngestorOpenSky.Models;
public interface IOpenSkyDataMapper
{
    List<KafkaEvent> MapToKafkaEvents(OpenSkyResponse response, Dictionary<string, string?> parametros);
}