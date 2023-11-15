using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NovelDocs.Entity;
using NovelDocs.Services;

namespace NovelDocs.Managers;

public interface IGoogleDocManager {
    Task<bool> GoogleDocExists(string googleDocId);
    Task<string> CreateDocument(IGoogleDocViewModel googleDocViewModel);
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
        var novel = _dataPersister.GetLastOpenedNovel();
        if (novel == null) {
            throw new Exception("No open novel found.");
        }

        var directoryId = await GetSubDirectory(novel, googleDocViewModel.GoogleDocType);
        return await _googleDocService.CreateDocument(directoryId, googleDocViewModel.Name);
    }

    private async Task<string> GetSubDirectory(Novel novel, GoogleDocType googleDocType) {
        var directoryId = (googleDocType == GoogleDocType.Character)
            ? novel.CharactersFolder
            : novel.ScenesFolder;

        if (!string.IsNullOrEmpty(directoryId)) {
            return directoryId;
        }

        var newDirectoryId = await _googleDocService.CreateDirectory(novel.GoogleDriveFolder, googleDocType.ToString() + "s");

        if (googleDocType == GoogleDocType.Character) {
            novel.CharactersFolder = newDirectoryId;
        } else {
            novel.ScenesFolder = newDirectoryId;
        }

        return newDirectoryId;
    }

    public async Task RenameDoc(string googleDocId, string name) {
        await _googleDocService.RenameDoc(googleDocId, name);
    }

    public async Task Compile() {
        var novel = _dataPersister.GetLastOpenedNovel();
        if (novel == null) {
            throw new Exception("No open novel found.");
        }

        var manuscriptId = novel.ManuscriptId;
        if (!await GoogleDocExists(manuscriptId)) {
            manuscriptId = await _googleDocService.CreateDocument(novel.GoogleDriveFolder, novel.Name);
            novel.ManuscriptId = manuscriptId;
            _dataPersister.Save();

        } else {
            await RenameDoc(manuscriptId, novel.Name);
            await _googleDocService.ClearDoc(manuscriptId);
        }

        await _googleDocService.Compile(manuscriptId, await GetDocIdsFromManuscriptElements(novel.ManuscriptElements));
    }

    private async Task<IList<string>> GetDocIdsFromManuscriptElements(IEnumerable<ManuscriptElement> manuscriptElements) {
        var docIds = new List<string>();
        foreach (var manuscriptElement in manuscriptElements) {
            if (manuscriptElement.Type != ManuscriptElementType.Scene) {
                if (manuscriptElement.IsChapter) {
                    docIds.Add($"Chapter:{manuscriptElement.Name}");
                }
                docIds.AddRange(await GetDocIdsFromManuscriptElements(manuscriptElement.ManuscriptElements));
                continue;
            }

            if (!await GoogleDocExists(manuscriptElement.GoogleDocId)) {
                continue;
            }

            docIds.Add(manuscriptElement.GoogleDocId);
        }

        return docIds;
    }
}