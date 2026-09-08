namespace ProducerOpenSky.Models;
public static class DeliveryHandlers
{
    public static Action<KafkaEvent> NoOp => _ => {};
}