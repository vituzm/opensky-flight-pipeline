namespace ProducerOpenSky.Interfaces;
public interface IEventFailureRepository
{
    void SaveMessageFailure(string key, byte[] value);
    Dictionary<string, byte[]> GetAllMessageFailures();
    void RemoveMessage(string key);
}