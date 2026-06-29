using IngestorOpenSky.Models;

namespace IngestorOpenSky.Interfaces;

public interface IKafkaProducerService
{
    void SendMessages(
        List<KafkaEvent> kafkaEvents, 
        Action<KafkaEvent> onSuccess,
        Action<KafkaEvent> onError
    );

    void SendMessage(
        KafkaEvent kafkaEvent, 
        Action<KafkaEvent> onSuccess,
        Action<KafkaEvent> onError
    );
}
