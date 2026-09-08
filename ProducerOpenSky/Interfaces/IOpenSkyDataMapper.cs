namespace ProducerOpenSky.Interfaces;
using ProducerOpenSky.Models;
public interface IOpenSkyDataMapper
{
    Task<List<KafkaEvent>> MapToKafkaEvents(OpenSkyResponse response, Dictionary<string, string?> parameters, string topic);
}