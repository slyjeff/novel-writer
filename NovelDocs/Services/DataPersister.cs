using System;
using System.IO;
using System.Linq;
using System.Windows.Markup;
using Newtonsoft.Json;
using NovelDocs.Entity;

namespace NovelDocs.Services;

public interface IDataPersister {
    public void Save();
    public Data Data { get; }
}

public static class DataPersisterExtensions {
    public static Novel? GetLastOpenedNovel(this IDataPersister dataPersister) {
        return dataPersister.Data.Novels.FirstOrDefault(x => x.Name == dataPersister.Data.LastOpenedNovel);
    }
}

internal sealed class DataPersister : IDataPersister {
    private const string FileName = "data.nd";
    private Data? _data;

    public void Save() {
        if (_data == null) {
            return;
        }

        var currentNovel = _data.Novels.FirstOrDefault(x => x.Name == _data.LastOpenedNovel);
        if (currentNovel != null) {
            currentNovel.LastModified = DateTime.Now;
        }

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