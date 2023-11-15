using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Docs.v1;
using Google.Apis.Docs.v1.Data;
using Google.Apis.Drive.v3;
using Google.Apis.Services;
using Google.Apis.Util;
using File = Google.Apis.Drive.v3.Data.File;
using Range = Google.Apis.Docs.v1.Data.Range;

namespace NovelDocs.Services;

public interface IGoogleDocService {
    Task<string> CreateDirectory(string parentId, string name);
    Task<string> CreateDocument(string directoryId, string name);
    Task<bool> GoogleDocExists (string googleDocId);
    Task RenameDoc(string googleDocId, string newName);
    Task ClearDoc(string manuscriptId);
    Task Compile(string documentId, IList<string> idsToCompile);
}

internal sealed class GoogleDocService : IGoogleDocService {
    public async Task<string> CreateDirectory(string parentId, string name) {
        var directory = new File {
            Name = name,
            MimeType = "application/vnd.google-apps.folder",
            Parents = new List<string> { parentId }
        };

        var credentials = await GetCredentials();
        var driveService = new DriveService(new BaseClientService.Initializer { HttpClientInitializer = credentials });
        var newDirectory = await driveService.Files.Create(directory).ExecuteAsync();

        return newDirectory.Id;
    }

    public async Task<string> CreateDocument(string directoryId, string name) {
        var doc = new File {
            Name = name,
            MimeType = "application/vnd.google-apps.document",
            Parents = new List<string> { directoryId }
        };

        var credentials = await GetCredentials();
        var driveService = new DriveService(new BaseClientService.Initializer { HttpClientInitializer = credentials });

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

    public async Task ClearDoc(string manuscriptId) {
        var credentials = await GetCredentials();
        var docsService = new DocsService(new BaseClientService.Initializer { HttpClientInitializer = credentials });
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

    public async Task Compile(string documentId, IList<string> idsToCompile) {
        var credentials = await GetCredentials();
        var docsService = new DocsService(new BaseClientService.Initializer { HttpClientInitializer = credentials });
        var fullManuscript = await docsService.Documents.Get(documentId).ExecuteAsync();

        var requests = new List<Request>();
        var currentPosition = 1;
        foreach (var docId in idsToCompile) {
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