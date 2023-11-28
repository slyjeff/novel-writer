using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;
using Newtonsoft.Json;
using NovelDocs.Entity;

namespace NovelDocs.Services;

public interface IDataPersister {
    Novel CurrentNovel { get; }
    Task AddNovel(IGoogleDirectory googleDirectory);
    public Task Save();
    public Data Data { get; }
    Task<bool> OpenNovel(NovelData? novelData = null);
    void CloseNovel();
    bool IsSaving { get; }

    event Action? OnFinishedSaving;
}

internal sealed class DataPersister : IDataPersister {
    private readonly IGoogleDocService _googleDocService;
    private const string FileName = "data.nd";
    private Data? _data;
    private OpenedNovel? _currentlyOpenedNovel;

    public DataPersister(IGoogleDocService googleDocService) {
        _googleDocService = googleDocService;
    }

    public Novel CurrentNovel => _currentlyOpenedNovel?.Novel ?? throw new Exception("Attempted to access novel when there is not open novel.");

    public async Task AddNovel(IGoogleDirectory googleDirectory) {
        var novel = new Novel {
            Name = googleDirectory.Name,
            GoogleDriveFolder = googleDirectory.Id,
        };

        var documentId = await _googleDocService.GetFileId("NovelDocs.dat", googleDirectory.Id);
        if (documentId == null) {
            documentId = await _googleDocService.CreateFile("NovelDocs.dat", googleDirectory.Id, JsonConvert.SerializeObject(novel));
        } else {
            var json = await _googleDocService.GetFileContents(documentId);
            novel = JsonConvert.DeserializeObject<Novel>(json) ?? novel;
        }

        var novelData = new NovelData {
            Name = googleDirectory.Name,
            GoogleId = documentId
        };

        Data.Novels.Add(novelData);

        await Save();

        _currentlyOpenedNovel = new OpenedNovel(novelData, novel);
        Data.LastOpenedNovel = novel.Name;
    }


    private readonly Timer _saveTimer = new(1000);
    public bool IsSaving => _saveTimer.Enabled;
    public event Action? OnFinishedSaving;

    public async Task Save() {
        if (_data == null) {
            return;
        }

        if (_currentlyOpenedNovel != null) {
            _currentlyOpenedNovel.NovelData.Name = _currentlyOpenedNovel.Novel.Name;
            _currentlyOpenedNovel.NovelData.LastModified = DateTime.Now;
            Data.LastOpenedNovel = _currentlyOpenedNovel.NovelData.Name;

            //for wait a second to send all updates at once.
            if (_saveTimer.Enabled) {
                _saveTimer.Stop();
            }
            else {
                _saveTimer.Elapsed += SaveNovel;
            }

            _saveTimer.Start();
        }

        var json = JsonConvert.SerializeObject(_data);
        await File.WriteAllTextAsync(FileName, json);
    }

    private async void SaveNovel(object? sender, ElapsedEventArgs e) {
        _saveTimer.Elapsed -= SaveNovel;
        _saveTimer.Stop();

        if (_currentlyOpenedNovel == null) {
            return;
        }

        await _googleDocService.UpdateFile(_currentlyOpenedNovel.NovelData.GoogleId, JsonConvert.SerializeObject(_currentlyOpenedNovel.Novel));
        OnFinishedSaving?.Invoke();
    }

    public async Task<bool> OpenNovel(NovelData? novelData = null) {
        if (novelData == null) {
            novelData = Data.Novels.FirstOrDefault(x => x.Name == Data.LastOpenedNovel);
            if (novelData == null) {
                return false;
            }
        }

        var json = await _googleDocService.GetFileContents(novelData.GoogleId);
        var novel = JsonConvert.DeserializeObject<Novel>(json);
        if (novel == null) {
            return false;
        }

        _currentlyOpenedNovel = new OpenedNovel(novelData, novel);
        Data.LastOpenedNovel = novel.Name;
        return true;
    }

    public void CloseNovel() {
        _currentlyOpenedNovel = null;
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

    private class OpenedNovel {
        public OpenedNovel(NovelData novelData, Novel novel) {
            NovelData = novelData;
            Novel = novel;
        }

        public NovelData NovelData { get; }
        public Novel Novel { get; }
    }
}