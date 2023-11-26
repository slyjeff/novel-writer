using System;
using System.IO;
using System.Threading.Tasks;
using NovelDocs.Entity;
using NovelDocs.Services;

namespace NovelDocs.Managers;

public interface IGoogleDocManager {
    Task<bool> GoogleDocExists(string googleDocId);
    Task<string> CreateDocument(IGoogleDocViewModel googleDocViewModel);
    Task UploadImage(string imagePath);
    Task DownloadImages();
    Task RenameDoc(string googleDocId, string name);
    Task Compile();
}

internal sealed class GoogleDocManager : IGoogleDocManager{
    private readonly IGoogleDocService _googleDocService;
    private readonly IDataPersister _dataPersister;

    public GoogleDocManager(IGoogleDocService googleDocService, IDataPersister dataPersister) {
        _googleDocService = googleDocService;
        _dataPersister = dataPersister;
    }

    public async Task<bool> GoogleDocExists(string googleDocId) {
        return await _googleDocService.GoogleDocExists(googleDocId);
    }

    public async Task<string> CreateDocument(IGoogleDocViewModel googleDocViewModel) {
        var directoryId = await GetSubDirectory(googleDocViewModel.GoogleDocType);
        return await _googleDocService.CreateDocument(directoryId, googleDocViewModel.Name);
    }
    public async Task UploadImage(string imagePath) {
        var directoryId = await GetSubDirectory(GoogleDocType.Image);
        
        if (await _googleDocService.GetFileId(Path.GetFileName(imagePath), directoryId) != null) {
            return;
        }
        await _googleDocService.UploadImage(directoryId, imagePath);
    }

    public async Task DownloadImages() {
        var novel = _dataPersister.CurrentNovel;
        if (novel == null) {
            throw new Exception("No open novel found.");
        }

        var directoryId = await GetSubDirectory(GoogleDocType.Image);

        foreach (var character in novel.Characters) {
            if (string.IsNullOrEmpty(character.ImageUriSource)) {
                continue;
            }

            if (File.Exists(character.ImageUriSource)) {
                continue;
            }

            await _googleDocService.DownloadImage(directoryId, character.ImageUriSource);
        }
    }

    private async Task<string> GetSubDirectory(GoogleDocType googleDocType) {
        var novel = _dataPersister.CurrentNovel;
        if (novel == null) {
            throw new Exception("No open novel found.");
        }

        var directoryId = novel.GetFolder(googleDocType);
        if (!string.IsNullOrEmpty(directoryId)) {
            return directoryId;
        }

        var newDirectoryId = await _googleDocService.CreateDirectory(novel.GoogleDriveFolder, googleDocType + "s");

        novel.SetFolder(googleDocType, newDirectoryId);
        
        return newDirectoryId;
    }

    public async Task RenameDoc(string googleDocId, string name) {
        await _googleDocService.RenameDoc(googleDocId, name);
    }

    public async Task Compile() {
        var novel = _dataPersister.CurrentNovel;
        if (novel == null) {
            throw new Exception("No open novel found.");
        }

        var manuscriptId = novel.ManuscriptId;
        if (!await GoogleDocExists(manuscriptId)) {
            manuscriptId = await _googleDocService.CreateDocument(novel.GoogleDriveFolder, novel.Name);
            novel.ManuscriptId = manuscriptId;
            await _dataPersister.Save();

        } else {
            await RenameDoc(manuscriptId, novel.Name);
            await _googleDocService.ClearDoc(manuscriptId);
        }

        await _googleDocService.Compile(manuscriptId, novel.ManuscriptElements.GetDocIds());
    }
}