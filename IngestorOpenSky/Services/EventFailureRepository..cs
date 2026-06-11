using IngestorOpenSky.Interfaces;
using RocksDbSharp;

namespace IngestorOpenSky.Services;

class EventFailureRepository : IEventFailureRepository, IDisposable
{
    private readonly DbOptions _options;
    private readonly string _path;
    private RocksDb _db;

    public EventFailureRepository()
    {
        _options = new DbOptions()
            .SetCreateIfMissing(true)
            .IncreaseParallelism(Environment.ProcessorCount);
        
        _path = Path.Combine(AppContext.BaseDirectory, "rocksdb_data");
        
        _db = RocksDb.Open(_options, _path);
    }

    public void SaveMessageFailure(string key, byte[] values)
    {
        throw new NotImplementedException();
    }


    public List<Dictionary<string, string>> GetAllMessageFailures()
    {
        throw new NotImplementedException();
    }

    public void DeleteMessage(string key)
    {
        throw new NotImplementedException();
    }

    public void Dispose()
    {
        _db?.Dispose();
    }
}