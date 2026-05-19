namespace IngestorOpenSky.Interfaces;
using IngestorOpenSky.Models;
public interface IFlightDataMapper
{
    List<KafkaEvent> MapToKafkaEvents(OpenSkyResponse response, Dictionary<string, string?> parametros);
}