namespace IngestorOpenSky.Models;
public static class DeliveryHandlers
{
    public static Action<KafkaEvent> NoOp => _ => {};
}