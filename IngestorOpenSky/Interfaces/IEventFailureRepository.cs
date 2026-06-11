namespace IngestorOpenSky.Interfaces;
public interface IEventFailureRepository
{
    void SaveMessageFailure(string key, byte[] value);
    List<Dictionary<string, string>> GetAllMessageFailures();
    void DeleteMessage(string key);
}