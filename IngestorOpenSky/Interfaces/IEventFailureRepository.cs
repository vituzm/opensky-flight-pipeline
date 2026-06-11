namespace IngestorOpenSky.Interfaces;
public interface IEventFailureRepository
{
    void SaveMessageFailure(Dictionary<string, byte[]> keyValues);
    List<Dictionary<string, string>> GetAllMessageFailures();
    void DeleteMessage(string key);
}