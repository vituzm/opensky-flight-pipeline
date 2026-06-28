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

    public void SaveMessageFailure(string key, string values)
    {
        _db.Put(key, values);
    }


    public Dictionary<string, string> GetAllMessageFailures()
    {
        var failedMessages = new Dictionary<string, string>(); 

        using var iterator = _db.NewIterator();
        
        for (iterator.SeekToFirst(); iterator.Valid(); iterator.Next())
        {
            failedMessages.Add(iterator.StringKey(), iterator.StringValue());
        }

        return failedMessages;
    }



    public void RemoveMessage(string key)
    {
        _db.Remove(key);
    }

    public void Dispose()
    {
        _db?.Dispose();
    }
}