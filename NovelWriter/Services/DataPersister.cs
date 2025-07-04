using System;
using System.Linq;
using System.Threading.Tasks;
using LiteDB;
using NovelWriter.Entity;
using Document = NovelWriter.Entity.Document;
using Task = System.Threading.Tasks.Task;

namespace NovelWriter.Services;

public interface IDataPersister {
    Novel CurrentNovel { get; }
    Task AddNovel();
    public Task Save();
    public Task SaveDocumentContent(IDocumentOwner documentOwner, string content);
    public Task<string> GetDocumentContent(IDocumentOwner documentOwner);
    public AppData AppData { get; }
    Task<bool> OpenNovel(NovelData? novelData = null);
    void CloseNovel();
    bool IsSaving { get; }
}

internal sealed class DataPersister : IDataPersister, IDisposable {
    private const string DatabaseFileName = "novelwriter.db";
    private readonly LiteDatabase _db = new(DatabaseFileName);
    private OpenedNovel? _currentlyOpenedNovel;

    private AppData? _appData;
    public AppData AppData {
        get { return _appData ??= LoadData(); }
    }

    private ILiteCollection<Novel>? _novels;

    private ILiteCollection<Novel> Novels {
        get { return _novels ??= _db.GetCollection<Novel>("novels"); }
    }

    
    private ILiteCollection<Document>? _documents;

    private ILiteCollection<Document> Documents {
        get { return _documents ??= _db.GetCollection<Document>("documents"); }
    }

    private ILiteCollection<DocumentVersion>? _documentVersions;
    private ILiteCollection<DocumentVersion> DocumentVersions {
        get { return _documentVersions ??= _db.GetCollection<DocumentVersion>("document_versions"); }
    }
    
    
    public Novel CurrentNovel => _currentlyOpenedNovel?.Novel ?? throw new Exception("Attempted to access novel when there is not open novel.");

    public async Task AddNovel() {
        var novel = new Novel();

        var novelData = new NovelData {
            Name = novel.Name,
        };

        AppData.Novels.Add(novelData);

        await Save();

        _currentlyOpenedNovel = new OpenedNovel(novelData, novel);
        AppData.LastOpenedNovel = novel.Name;
    }

    public async Task Save() {
        if (_appData == null) {
            return;
        }

        await Task.Run(() => {
            IsSaving = true;
            try {
                SaveNovel();
                SaveAppData();
            } finally {
                IsSaving = false;
            }
        });
    }

    public async Task SaveDocumentContent(IDocumentOwner documentOwner, string content) {
        await Task.Run(() => {
            if (!_db.BeginTrans())
                throw new InvalidOperationException("Could not begin transaction");

            try {
                Document? document = null;
                if (documentOwner.DocumentId > 0) {
                    document = Documents.FindById(documentOwner.DocumentId);
                }

                if (document == null) {
                    CreateDocument(documentOwner, content);
                } else {
                    UpdateDocument(document, content);
                }
                
                _db.Commit();
            } catch {
                _db.Rollback();
                throw;
            }
        });
    }

    private void CreateDocument(IDocumentOwner documentOwner, string content) {
        var document = new Document { Content = content };
        Documents.Insert(document);
    
        documentOwner.DocumentId = document.Id;
        if (_currentlyOpenedNovel != null) {
            Novels.Upsert(_currentlyOpenedNovel.Novel);
        }

        var documentVersion = new DocumentVersion { DocumentId = document.Id, Content = content };
        DocumentVersions.Insert(documentVersion);
    }

    private void UpdateDocument(Document document, string content) {
        document.Content = content;
        document.ModifiedDate = DateTime.UtcNow;
        Documents.Update(document);

        var currentVersion = DocumentVersions
            .Query()
            .Where(x => x.DocumentId == document.Id)
            .OrderByDescending(x => x.Version)
            .FirstOrDefault() ?? new DocumentVersion {
            Version = 0,
            VersionDate = DateTime.MinValue
        };

        if (currentVersion.VersionDate > DateTime.UtcNow.AddMinutes(-5)) {
            return;
        }

        var documentVersion = new DocumentVersion {
            DocumentId = document.Id,
            Content = content,
            Version = currentVersion.Version + 1
        };
        DocumentVersions.Insert(documentVersion);
    }
    
    public async Task<string> GetDocumentContent(IDocumentOwner documentOwner) {
        return await Task.Run(() => {
            if (documentOwner.DocumentId == 0) {
                return string.Empty;
            }
            
            var document = Documents.FindById(documentOwner.DocumentId);
            return document == null ? string.Empty : document.Content;
        });
    }

    private void SaveNovel() {
        if (_currentlyOpenedNovel == null) {
            return;
        }

        var novelData = _currentlyOpenedNovel.NovelData;
        novelData.Name = _currentlyOpenedNovel.Novel.Name;
        novelData.LastModified = DateTime.Now;
        AppData.LastOpenedNovel = novelData.Name;

        Novels.Upsert(_currentlyOpenedNovel.Novel);
    }

    private void SaveAppData() {
        if (_appData == null) {
            return;
        }
        
        var appData = _db.GetCollection<AppData>("app_data");
        appData.Upsert(_appData);
    }

    public async Task<bool> OpenNovel(NovelData? novelData = null) {
        if (novelData == null) {
            novelData = AppData.Novels.FirstOrDefault(x => x.Name == AppData.LastOpenedNovel);
            if (novelData == null)
            {
                return false;
            }
        }
        
        //var oldNovel = await Task.Run(() => _db.GetCollection<OldNovel>("old_novels").FindOne(x => x.Name == novelData.Name)) ?? new OldNovel { Name = novelData.Name };
        var novel = await Task.Run(() => Novels.FindOne(x => x.Name == novelData.Name)) ?? new Novel { Name = novelData.Name };
        /*
        novel.Characters = oldNovel.Characters;
        novel.ManuscriptElements = oldNovel.ManuscriptElements;
        novel.Events = oldNovel.Events;
        novel.EventBoardCharacters = oldNovel.EventBoardCharacters;
        novel.SupportDocuments = oldNovel.SupportDocuments;
        */
        
        _currentlyOpenedNovel = new OpenedNovel(novelData, novel);
        AppData.LastOpenedNovel = novel.Name;
        
        return true;
    }

    public void CloseNovel() {
        _currentlyOpenedNovel = null;
    }

    public bool IsSaving { get; private set; }

    private AppData LoadData() {
        var appData = _db.GetCollection<AppData>("app_data");
        return appData.FindOne(Query.All()) ?? new AppData();
    }

    private class OpenedNovel(NovelData novelData, Novel novel) {
        public NovelData NovelData { get; } = novelData;
        public Novel Novel { get; } = novel;
    }

    public void Dispose() {
        _db.Dispose();
    }
}
