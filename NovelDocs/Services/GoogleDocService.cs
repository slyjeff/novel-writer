using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Docs.v1;
using Google.Apis.Docs.v1.Data;
using Google.Apis.Drive.v3;
using Google.Apis.Drive.v3.Data;
using Google.Apis.Services;
using Google.Apis.Util;
using NovelDocs.Entity;
using Range = Google.Apis.Docs.v1.Data.Range;

namespace NovelDocs.Services;

public interface IGoogleDocService {
    Task<bool> GoogleDocExists (string googleDocId);
    Task<string> CreateDocument(IGoogleDocViewModel googleDocViewModel);
    Task RenameDoc(string googleDocId, string newName);
    Task Compile();
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

    public async Task Compile() {
        var novel = _dataPersister.GetLastOpenedNovel();
        if (novel == null) {
            return;
        }

        var credentials = await GetCredentials();
        var initializer = new BaseClientService.Initializer { HttpClientInitializer = credentials };
        var driveService = new DriveService(initializer);
        var docsService = new DocsService(initializer);

        var manuscriptId = novel.ManuscriptId;
        if (!await GoogleDocExists(manuscriptId)) {
            var doc = new File {
                Name = novel.Name,
                MimeType = "application/vnd.google-apps.document",
                Parents = new List<string> { novel.GoogleDriveFolder }
            };

            var newDoc = await driveService.Files.Create(doc).ExecuteAsync();
            novel.ManuscriptId = newDoc.Id;
            _dataPersister.Save();

            manuscriptId = newDoc.Id;
        } else {
            await ClearExistingManuscript(manuscriptId, novel.Name);
        }

        var requests = new List<Request>();
        var fullManuscript = await docsService.Documents.Get(manuscriptId).ExecuteAsync();

        var currentPosition = 1;
        var docs = await GetDocIdsFromManuscriptElements(novel.ManuscriptElements);
        foreach (var docId in await GetDocIdsFromManuscriptElements(novel.ManuscriptElements)) {
            if (currentPosition != 1) {
                //check if we need to add a page break or line break
                if (string.IsNullOrEmpty(docId)) {
                    requests.Add(new Request {
                        InsertPageBreak = new InsertPageBreakRequest {
                            EndOfSegmentLocation = new EndOfSegmentLocation()
                        }
                    });
                    currentPosition += 2;
                } else {
                    requests.Add(new Request {
                        InsertText = new InsertTextRequest {
                            Text = "\n",
                            Location = new Location {
                                Index = currentPosition
                            }
                        }
                    });
                    currentPosition++;
                }
            }

            if (string.IsNullOrEmpty(docId)) {
                continue;
            }

            var document = await docsService.Documents.Get(docId).ExecuteAsync();
            if (document == null) {
                continue;
            }

            foreach (var contentItem in document.Body.Content) {
                if (contentItem.Paragraph == null) {
                    continue;
                }

                if (contentItem.Paragraph.ParagraphStyle != null) {
                    requests.Add(new Request {
                        UpdateParagraphStyle = new UpdateParagraphStyleRequest {
                            Range = new Range {
                                StartIndex = currentPosition,
                                EndIndex = currentPosition
                            },
                            ParagraphStyle = contentItem.Paragraph.ParagraphStyle,
                            Fields = "*"
                        }
                    });
                }

                foreach (var element in contentItem.Paragraph.Elements) {
                    if (element.PageBreak != null) {
                        requests.Add(new Request {
                            InsertPageBreak = new InsertPageBreakRequest {
                                Location = new Location {
                                    Index = currentPosition
                                }
                            }
                        });
                        currentPosition += 2;
                    }

                    if (element.TextRun == null) {
                        continue;
                    }

                    requests.Add(new Request {
                        InsertText = new InsertTextRequest {
                            Text = element.TextRun.Content,
                            Location = new Location {
                                Index = currentPosition
                            }
                        }
                    });

                    if (element.TextRun.Content.Length > 0) {
                        requests.Add(new Request {
                            UpdateTextStyle = new UpdateTextStyleRequest {
                                Range = new Range {
                                    StartIndex = currentPosition,
                                    EndIndex = currentPosition + element.TextRun.Content.Length
                                },
                                TextStyle = element.TextRun.TextStyle,
                                Fields = "*"
                            }
                        });
                    }

                    currentPosition += element.TextRun.Content.Length;
                }
            }
        }

        var batchUpdate = new BatchUpdateDocumentRequest {
            Requests = requests
        };

        await docsService.Documents.BatchUpdate(batchUpdate, fullManuscript.DocumentId).ExecuteAsync();
    }

    private async Task ClearExistingManuscript(string manuscriptId, string novelName) {
        var credentials = await GetCredentials();
        var docsService = new DocsService(new BaseClientService.Initializer { HttpClientInitializer = credentials });
        await RenameDoc(manuscriptId, novelName);
        var requests = new List<Request>();
        var fullManuscript = await docsService.Documents.Get(manuscriptId).ExecuteAsync();
        var existingLength = fullManuscript.Body.Content.Sum(GetLengthOfStructuralElement);
        if (existingLength <= 2) {
            return;
        }

        requests.Add(new Request {
            DeleteContentRange = new DeleteContentRangeRequest {
                Range = new Range {
                    StartIndex = 1,
                    EndIndex = existingLength - 1
                }
            }
        });

        var batchUpdate = new BatchUpdateDocumentRequest {
            Requests = requests
        };

        await docsService.Documents.BatchUpdate(batchUpdate, fullManuscript.DocumentId).ExecuteAsync();
    }

    private async Task<IList<string>> GetDocIdsFromManuscriptElements(IEnumerable<ManuscriptElement> manuscriptElements) {
        var docIds = new List<string>();
        var sceneHasBeenInsertAtThisLevel = false;
        foreach (var manuscriptElement in manuscriptElements) {
            if (manuscriptElement.Type != ManuscriptElementType.Scene) {
                docIds.AddRange(await GetDocIdsFromManuscriptElements(manuscriptElement.ManuscriptElements));
                continue;
            }

            if (!await GoogleDocExists(manuscriptElement.GoogleDocId)) {
                continue;
            }

            if (!sceneHasBeenInsertAtThisLevel) {
                docIds.Add(string.Empty);
                sceneHasBeenInsertAtThisLevel = true;
            }

            docIds.Add(manuscriptElement.GoogleDocId);
        }

        return docIds;
    }

    private static int GetLengthOfStructuralElement(StructuralElement element) {
        if (element.Paragraph == null) {
            return 1;
        }

        var length = 0;
        foreach (var paragraphElement in element.Paragraph.Elements) {
            if (paragraphElement.PageBreak != null) {
                length++;
                continue;
            }

            if (paragraphElement.TextRun?.Content == null) {
                continue;
            }

            length += paragraphElement.TextRun.Content.Length;
        }

        return length;
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