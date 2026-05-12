using IngestorOpenSky.Models;

namespace IngestorOpenSky.Interfaces;

public interface IKafkaProducerService
{
    void EnviarMensagensOpenSky(OpenSkyResponse response, Dictionary<string, string?> parametros);
}
