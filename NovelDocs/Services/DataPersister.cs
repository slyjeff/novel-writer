using System.IO;
using Newtonsoft.Json;
using NovelDocs.Entity;

namespace NovelDocs.Services;

public interface IDataPersister {
    public void Save();
    public Data Data { get; }
}

internal sealed class DataPersister : IDataPersister {
    private const string FileName = "data.nd";
    private Data? _data;

    public void Save() {
        var json = JsonConvert.SerializeObject(_data);
        File.WriteAllText(FileName, json);
    }

    public Data Data {
        get { return _data ??= LoadData(); }
    }

    private static Data LoadData() {
        if (!File.Exists(FileName)) {
            return new Data();
        }

        var fileText = File.ReadAllText(FileName);
        return JsonConvert.DeserializeObject<Data>(fileText) ?? new Data();
    }
}