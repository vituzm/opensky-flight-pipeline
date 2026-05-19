using IngestorOpenSky.Models;

namespace IngestorOpenSky.Interfaces;

public interface IKafkaProducerService
{
    void EnviarMensagensOpenSky(List<KafkaEvent> kafkaEvents, string topicName);
}
