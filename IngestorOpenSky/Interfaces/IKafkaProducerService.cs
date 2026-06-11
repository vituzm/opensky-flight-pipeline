using IngestorOpenSky.Models;

namespace IngestorOpenSky.Interfaces;

public interface IKafkaProducerService
{
    void SendMessages(List<KafkaEvent> kafkaEvents);
}
