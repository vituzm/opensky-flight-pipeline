namespace IngestorOpenSky.Interfaces;
public interface IEventFailureRepository
{
    void SaveMessageFailure(string key, string value);
    Dictionary<string, string> GetAllMessageFailures();
    void RemoveMessage(string key);
}