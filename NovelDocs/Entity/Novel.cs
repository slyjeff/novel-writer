using CefSharp.DevTools.DOM;
using NovelDocs.PageControls;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NovelDocs.Entity; 

public sealed class Novel {
    public string Name { get; set; } = "New Novel";
    public string Author { get; set; } = string.Empty;
    public string CopyrightYear { get; set; } = DateTime.Today.Year.ToString();
    public string GoogleDriveFolder { get; set; } = string.Empty;
    public string ScenesFolder { get; set; } = string.Empty;
    public string ImagesFolder { get; set; } = string.Empty;
    public string CharactersFolder { get; set; } = string.Empty;
    public DateTime LastModified { get; set; } = DateTime.Now;
    public string ManuscriptId { get; set; } = string.Empty;
    public Typesetting Typesetting { get; set; } = new Typesetting();
    
    public IList<ManuscriptElement> ManuscriptElements { get; set; } = new List<ManuscriptElement>();

    public IList<Character> Characters { get; set; } = new List<Character>();

    public IList<Event> Events { get; set; } = new List<Event>();

    public IList<EventBoardCharacter> EventBoardCharacters { get; set; } = new List<EventBoardCharacter>();

    public string GetFolder(GoogleDocType googleDocType) {
        return googleDocType switch {
            GoogleDocType.Scene => ScenesFolder,
            GoogleDocType.Character => CharactersFolder,
            GoogleDocType.Image => ImagesFolder,
            _ => throw new ArgumentOutOfRangeException(nameof(googleDocType), googleDocType, null)
        };
    }

    public void SetFolder(GoogleDocType googleDocType, string newDirectoryId) {
        switch (googleDocType) {
            case GoogleDocType.Character:
                CharactersFolder = newDirectoryId;
                break;
            case GoogleDocType.Scene:
                ScenesFolder = newDirectoryId;
                break;
            case GoogleDocType.Image:
                ImagesFolder = newDirectoryId;
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(googleDocType), googleDocType, null);
        }
    }

    public Character? FindCharacterById(Guid id) {
        return Characters.FirstOrDefault(x => x.Id == id);
    }
}

public sealed class Typesetting {
    public string TitleFont { get; set; } = string.Empty;
    public string HeaderFont { get; set; } = string.Empty;
    public int HeaderFontSize { get; set; } = 12;
    public string PageNumberFont { get; set; } = string.Empty;
    public int PageNumberFontSize { get; set; } = 12;
    public string ChapterFont { get; set; } = string.Empty;
    public string BodyFont { get; set; } = string.Empty;
    public int BodyFontSize { get; set; } = 12;
}
