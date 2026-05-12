using IngestorOpenSky.Models;

namespace IngestorOpenSky.Interfaces;

public interface IKafkaProducerService
{
    void EnviarMensagensOpenSky(OpenSkyResponse response, IReadOnlyDictionary<string, string?> parametros);
}
