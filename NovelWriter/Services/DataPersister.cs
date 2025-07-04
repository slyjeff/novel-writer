using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using NovelWriter.Entity;

namespace NovelWriter.Services;

public interface IDataPersister {
    Novel CurrentNovel { get; }
    Task AddNovel();
    public Task Save();
    public NovelList NovelList { get; }
    Task<bool> OpenNovel(NovelData? novelData = null);
    void CloseNovel();
    bool IsSaving { get; }
}

internal sealed class DataPersister : IDataPersister {
    private const string NovelListFileName = "data.nd";
    private NovelList? _novelList;
    private OpenedNovel? _currentlyOpenedNovel;

    public Novel CurrentNovel => _currentlyOpenedNovel?.Novel ?? throw new Exception("Attempted to access novel when there is not open novel.");

    public async Task AddNovel() {
        var novel = new Novel();

        var novelData = new NovelData {
            Name = novel.Name,
        };

        NovelList.Novels.Add(novelData);

        await Save();

        _currentlyOpenedNovel = new OpenedNovel(novelData, novel);
        NovelList.LastOpenedNovel = novel.Name;
    }

    public async Task Save() {
        if (_novelList == null) {
            return;
        }

        await SaveNovel();
        await SaveNovelList();
    }

    private async Task SaveNovel() {
        if (_currentlyOpenedNovel == null) {
            return;
        }

        var novelData = _currentlyOpenedNovel.NovelData;
        var originalFilename = novelData.FileName;
        novelData.Name = _currentlyOpenedNovel.Novel.Name;
        novelData.LastModified = DateTime.Now;
        NovelList.LastOpenedNovel = novelData.Name;

        await WriteFile(novelData.FileName, _currentlyOpenedNovel.Novel);
        
        if (originalFilename != novelData.FileName) {
            File.Delete(originalFilename);
        }
    }

    private async Task SaveNovelList() {
        await WriteFile(NovelListFileName, _novelList);
    }

    private async Task WriteFile(string path, object? data) {
        const int maxRetries = 3;
        const int baseDelayMs = 100;
    
        if (data == null) {
            return;
        }

        var json = JsonConvert.SerializeObject(data);
        for (var attempt = 0; attempt <= maxRetries; attempt++) {
            try {
                await File.WriteAllTextAsync(path, json);
                return;
            }
            catch (Exception) when (attempt < maxRetries) {
                await Task.Delay(baseDelayMs * (int)Math.Pow(2, attempt));
            }
        }
        
        throw new IOException($"Failed to save {path} after {maxRetries} attempts");
    }

    public async Task<bool> OpenNovel(NovelData? novelData = null) {
        if (novelData == null) {
            novelData = NovelList.Novels.FirstOrDefault(x => x.Name == NovelList.LastOpenedNovel);
            if (novelData == null) {
                return false;
            }
        }

        var novel = new Novel { Name = novelData.Name };
        if (File.Exists(novelData.FileName)) {
            var json = await File.ReadAllTextAsync(novelData.FileName);
            novel = JsonConvert.DeserializeObject<Novel>(json);
            if (novel == null) {
                return false;
            }
        }

        _currentlyOpenedNovel = new OpenedNovel(novelData, novel);

        NovelList.LastOpenedNovel = novel.Name;
        
        return true;
    }

    public void CloseNovel() {
        _currentlyOpenedNovel = null;
    }

    public bool IsSaving => false;

    public NovelList NovelList {
        get { return _novelList ??= LoadData(); }
    }

    private static NovelList LoadData() {
        if (!File.Exists(NovelListFileName)) {
            return new NovelList();
        }

        var fileText = File.ReadAllText(NovelListFileName);
        return JsonConvert.DeserializeObject<NovelList>(fileText) ?? new NovelList();
    }

    private class OpenedNovel(NovelData novelData, Novel novel) {
        public NovelData NovelData { get; } = novelData;
        public Novel Novel { get; } = novel;
    }
}