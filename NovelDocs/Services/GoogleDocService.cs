using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Docs.v1;
using Google.Apis.Docs.v1.Data;
using Google.Apis.Services;
using Google.Apis.Util;

namespace NovelDocs.Services;

public interface IGoogleDocService {
    Task<bool> GoogleDocExists (string googleDocId);
    Task<string> CreateDocument(string title);
}


internal sealed class GoogleDocService : IGoogleDocService {
    public async Task<string> CreateDocument(string title) {
        var credentials = await GetCredentials();

        var docsService = new DocsService(new BaseClientService.Initializer { HttpClientInitializer = credentials });
        var document = new Document {
            Title = title
        };

        var createdDocument = await docsService.Documents.Create(document).ExecuteAsync();
        return createdDocument.DocumentId;
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
        if (!File.Exists("ClientSecret.txt")) {
            throw new Exception("'ClientSecret.txt' file not found.");
        }

        return File.ReadAllText("ClientSecret.txt");
    }
}