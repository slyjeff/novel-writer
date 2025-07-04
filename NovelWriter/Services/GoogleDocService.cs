using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Docs.v1;
using Google.Apis.Docs.v1.Data;
using Google.Apis.Drive.v3;
using Google.Apis.Services;
using Google.Apis.Upload;
using Google.Apis.Util;
using File = Google.Apis.Drive.v3.Data.File;
using Range = Google.Apis.Docs.v1.Data.Range;

namespace NovelWriter.Services;

public interface IGoogleDirectory {
    string Id { get; }
    string Name { get; }
}

internal sealed class GoogleDirectory : IGoogleDirectory {
    public GoogleDirectory(File directory) {
        Id = directory.Id;
        Name = directory.Name;
    }

    public string Id { get; }
    public string Name { get; }
    public override string ToString() {
        return Name;
    }
}

public interface IGoogleDocService {
    Task<IList<IGoogleDirectory>> GetDirectoryList(string? parentId = null);
    Task<string> CreateFile(string filename, string parentId, string data);
    Task UpdateFile(string fileId, string newData);
    Task UploadImage(string parentId, string imagePaths);
    Task DownloadImage(string directoryId, string filePath);
    Task<string?> GetFileId(string name, string parentId);
    Task<string> GetFileContents(string id);
    Task<string> CreateDirectory(string parentId, string name);
    Task<string> CreateDocument(string directoryId, string name);
    Task<bool> GoogleDocExists(string googleDocId);
    Task<Document?> GetGoogleDoc(string googleDocId);
    Task RenameDoc(string googleDocId, string newName);
    Task ClearDoc(string manuscriptId);
    Task Compile(string documentId, IList<string> idsToCompile);
}

internal sealed class GoogleDocService : IGoogleDocService {
    public async Task<IList<IGoogleDirectory>> GetDirectoryList(string? parentId = null) {
        var credentials = await GetCredentials();
        var driveService = new DriveService(new BaseClientService.Initializer { HttpClientInitializer = credentials });
        var listRequest = driveService.Files.List();

        parentId ??= "root";
        listRequest.Q = $"'{parentId}' in parents and mimeType = 'application/vnd.google-apps.folder'";

        var files = await listRequest.ExecuteAsync();
        return files.Files.Select(x => (IGoogleDirectory)new GoogleDirectory(x)).ToList();
    }

    public async Task<string> CreateFile(string name, string parentId, string data) {
        var file = new File {
            Name = name,
            Parents = new List<string> { parentId }
        };

        var credentials = await GetCredentials();
        var driveService = new DriveService(new BaseClientService.Initializer { HttpClientInitializer = credentials });

        using (var stream = new MemoryStream(Encoding.ASCII.GetBytes(data))) {
            var request = driveService.Files.Create(file, stream, "application/octet-stream");
            request.Fields = "id";

            var response = await request.UploadAsync();
            if (response.Status != UploadStatus.Completed) {
                throw new Exception($"Upload of file {name} failed.");
            }
            return request.ResponseBody.Id;
        }
    }

    public async Task UpdateFile(string fileId, string newData) {
        var credentials = await GetCredentials();
        var driveService = new DriveService(new BaseClientService.Initializer { HttpClientInitializer = credentials });

        var file = await driveService.Files.Get(fileId).ExecuteAsync();
        file.Id = null;

        using (var stream = new MemoryStream(Encoding.ASCII.GetBytes(newData))) {
            var result = await driveService.Files.Update(file, fileId, stream, "application/octet-stream").UploadAsync();
            if (result.Status != UploadStatus.Completed) {
                throw new Exception("Error uploading file to Google Docs.");
            }
        }
    }

    public async Task UploadImage(string parentId, string imagePath) {
        var fileName = Path.GetFileName(imagePath);
        var extension = Path.GetExtension(imagePath)[1..]; //remove the dot

        var file = new File {
            Name = fileName,
            Parents = new List<string> { parentId }
        };

        var credentials = await GetCredentials();
        var driveService = new DriveService(new BaseClientService.Initializer { HttpClientInitializer = credentials });

        using (var stream = new FileStream(imagePath, FileMode.Open)) {
            var request = driveService.Files.Create(file, stream, $"image/{extension}");

            var response = await request.UploadAsync();
            if (response.Status != UploadStatus.Completed) {
                throw new Exception($"Upload of file {imagePath} failed.");
            }
        }
    }

    public async Task DownloadImage(string directoryId, string filePath) {
        var fileName = Path.GetFileName(filePath);
        var imageId = await GetFileId(fileName, directoryId);
        if (string.IsNullOrEmpty(imageId)) {
            return;
        }

        var credentials = await GetCredentials();
        var driveService = new DriveService(new BaseClientService.Initializer { HttpClientInitializer = credentials });

        using (var outputStream = new FileStream(filePath, FileMode.Create)) {
            await driveService.Files.Get(imageId).DownloadAsync(outputStream);
        }
    }

    public async Task<string?> GetFileId(string name, string parentId) {
        var credentials = await GetCredentials();
        var driveService = new DriveService(new BaseClientService.Initializer { HttpClientInitializer = credentials });
        var listRequest = driveService.Files.List();

        listRequest.Q = $"'{parentId}' in parents and name = '{name}'";

        var files = await listRequest.ExecuteAsync();
        return files.Files.FirstOrDefault()?.Id;
    }

    public async Task<string> GetFileContents(string fileId) {
        var credentials = await GetCredentials();
        var driveService = new DriveService(new BaseClientService.Initializer { HttpClientInitializer = credentials });

        using (var outputStream = new MemoryStream()) {
            await driveService.Files.Get(fileId).DownloadAsync(outputStream);
            return Encoding.ASCII.GetString(outputStream.ToArray());
        }
    }

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
        if (!await GoogleDocExists(googleDocId)) {
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
        return await GetGoogleDoc(googleDocId) != null;
    }

    public async Task<Document?> GetGoogleDoc(string googleDocId) {
        if (string.IsNullOrEmpty(googleDocId)) {
            return null;
        }

        var credentials = await GetCredentials();
        var docsService = new DocsService(new BaseClientService.Initializer { HttpClientInitializer = credentials });
        try {
            return await docsService.Documents.Get(googleDocId).ExecuteAsync();
        } catch {
            return null;
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
        var addBreakBeforeNextSection = false;
        foreach (var docId in idsToCompile) {
            if (string.IsNullOrEmpty(docId)) {
                continue;
            }

            var isNewChapter = docId.StartsWith("Chapter:");
            if (isNewChapter) {
                if (currentPosition != 1) {
                    requests.Add(new Request {
                        InsertPageBreak = new InsertPageBreakRequest {
                            EndOfSegmentLocation = new EndOfSegmentLocation()
                        }
                    });
                    currentPosition += 2;
                }
            } else if (addBreakBeforeNextSection) {
                requests.Add(new Request {
                    InsertText = new InsertTextRequest {
                        Text = "\n",
                        Location = new Location {
                            Index = currentPosition
                        }
                    }
                });
                currentPosition++;

                var text = "* * *\n";
                requests.Add(new Request {
                    InsertText = new InsertTextRequest {
                        Text = text,
                        Location = new Location {
                            Index = currentPosition
                        }
                    }
                });

                requests.Add(new Request {
                    UpdateParagraphStyle = new UpdateParagraphStyleRequest {
                        Range = new Range {
                            StartIndex = currentPosition,
                            EndIndex = currentPosition + text.Length
                        },
                        ParagraphStyle = new ParagraphStyle { Alignment = "CENTER", Direction = "LEFT_TO_RIGHT" },
                        Fields = "Alignment,Direction"
                    }
                });
                currentPosition += text.Length;

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

            if (isNewChapter) {
                var text = docId["Chapter:".Length..] + "\n";

                requests.Add(new Request {
                    InsertText = new InsertTextRequest {
                        Text = text,
                        Location = new Location {
                            Index = currentPosition
                        }
                    }
                });

                requests.Add(new Request {
                    UpdateParagraphStyle = new UpdateParagraphStyleRequest {
                        Range = new Range {
                            StartIndex = currentPosition,
                            EndIndex = currentPosition
                        },
                        ParagraphStyle = new ParagraphStyle { NamedStyleType = "HEADING_1" },
                        Fields = "NamedStyleType"
                    }
                });

                currentPosition += text.Length;
                addBreakBeforeNextSection = false;

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
                    addBreakBeforeNextSection = true;
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