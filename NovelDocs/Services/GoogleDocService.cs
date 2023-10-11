using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Docs.v1;
using Google.Apis.Drive.v3;
using Google.Apis.Drive.v3.Data;
using Google.Apis.Services;
using Google.Apis.Util;

namespace NovelDocs.Services;

public interface IGoogleDocService {
    Task<bool> GoogleDocExists (string googleDocId);
    Task<string> CreateDocument(IGoogleDocViewModel googleDocViewModel);
    Task RenameDoc(string googleDocId, string newName);
}


internal sealed class GoogleDocService : IGoogleDocService {
    private readonly IDataPersister _dataPersister;

    public GoogleDocService(IDataPersister dataPersister) {
        _dataPersister = dataPersister;
    }

    public async Task<string> CreateDocument(IGoogleDocViewModel googleDocViewModel) {

        var credentials = await GetCredentials();
        var driveService = new DriveService(new BaseClientService.Initializer { HttpClientInitializer = credentials });
        var directory = await GetSubDirectory(googleDocViewModel.GoogleDocType);

        var doc = new File {
            Name = googleDocViewModel.Name,
            MimeType = "application/vnd.google-apps.document",
            Parents = new List<string> { directory }
        };

        var newDoc = await driveService.Files.Create(doc).ExecuteAsync();
        return newDoc.Id;
    }

    public async Task RenameDoc(string googleDocId, string newName) {
        if (! await GoogleDocExists(googleDocId)) {
            return;
        }

        var credentials = await GetCredentials();
        var driveService = new DriveService(new BaseClientService.Initializer { HttpClientInitializer = credentials });

        var file = await driveService.Files.Get(googleDocId).ExecuteAsync();
        file.Name = newName;
        file.Id = null;
        await driveService.Files.Update(file, googleDocId).ExecuteAsync();
    }

    public async Task<bool> GoogleDocExists(string googleDocId) {
        if (string.IsNullOrEmpty(googleDocId)) {
            return false;
        }

        var credentials = await GetCredentials();

        var docsService = new DocsService(new BaseClientService.Initializer { HttpClientInitializer = credentials });
        try {
            return (await docsService.Documents.Get(googleDocId).ExecuteAsync() != null);
        } catch {
            return false;
        }
    }


    private async Task<string> GetSubDirectory(GoogleDocType googleDocType) {
        var novel = _dataPersister.GetLastOpenedNovel();
        if (novel == null) {
            return string.Empty;
        }

        var directoryId = (googleDocType == GoogleDocType.Character)
            ? novel.CharactersFolder
            : novel.ScenesFolder;

        if (!string.IsNullOrEmpty(directoryId)) {
            return directoryId;
        }

        var directory = new File {
            Name = googleDocType.ToString(),
            MimeType = "application/vnd.google-apps.folder",
            Parents = new List<string> {novel.GoogleDriveFolder}
        };

        var credentials = await GetCredentials();
        var driveService = new DriveService(new BaseClientService.Initializer { HttpClientInitializer = credentials });
        var newDirectory = await driveService.Files.Create(directory).ExecuteAsync();

        if (googleDocType == GoogleDocType.Character) {
            novel.CharactersFolder = newDirectory.Id;
        } else {
            novel.ScenesFolder = newDirectory.Id;
        }

        return newDirectory.Id;
    }

    private static async Task<UserCredential> GetCredentials() {
        var clientSecrets = new ClientSecrets {
            ClientId = "905518365711-3injjl7unepaof6alnan5cueh8j57hs3.apps.googleusercontent.com",
            ClientSecret = GetClientSecret()
        };

        string[] scopes = {
            "https://www.googleapis.com/auth/documents", 
            "https://www.googleapis.com/auth/drive", 
            "https://www.googleapis.com/auth/drive.file", 
            "https://www.googleapis.com/auth/drive.appdata"
        };

        var credentials = await GoogleWebAuthorizationBroker.AuthorizeAsync(clientSecrets, scopes, "User", CancellationToken.None);
        if (credentials.Token.IsExpired(SystemClock.Default)) {
            credentials.RefreshTokenAsync(CancellationToken.None).Wait();
        }

        return credentials;
    }

    private static string GetClientSecret() {
        if (!System.IO.File.Exists("ClientSecret.txt")) {
            throw new Exception("'ClientSecret.txt' file not found.");
        }

        return System.IO.File.ReadAllText("ClientSecret.txt");
    }
}