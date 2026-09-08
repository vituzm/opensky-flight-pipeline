using ProducerOpenSky.Models;

namespace ProducerOpenSky.Interfaces;

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
