using System;
using System.Collections.Generic;
using System.Linq;

namespace NovelWriter.Entity; 

public sealed class Novel {
    public string Name { get; set; } = "New Novel";
    public string Author { get; set; } = string.Empty;
    public string CopyrightYear { get; set; } = DateTime.Today.Year.ToString();
    public string GoogleDriveFolder { get; set; } = string.Empty;
    public string ScenesFolder { get; set; } = string.Empty;
    public string ImagesFolder { get; set; } = string.Empty;
    public string CharactersFolder { get; set; } = string.Empty;
    public string SupportDocumentsFolder { get; set; } = string.Empty;
    public string ManuscriptId { get; set; } = string.Empty;
    public Typesetting Typesetting { get; set; } = new Typesetting();
    
    public IList<ManuscriptElement> ManuscriptElements { get; set; } = new List<ManuscriptElement>();

    public IList<Character> Characters { get; set; } = new List<Character>();

    public IList<Event> Events { get; set; } = new List<Event>();

    public IList<EventBoardCharacter> EventBoardCharacters { get; set; } = new List<EventBoardCharacter>();

    public IList<SupportDocument> SupportDocuments { get; set; } = new List<SupportDocument>();

    public string GetFolder(GoogleDocType googleDocType) {
        return googleDocType switch {
            GoogleDocType.Scene => ScenesFolder,
            GoogleDocType.Character => CharactersFolder,
            GoogleDocType.SupportDocument => SupportDocumentsFolder,
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
            case GoogleDocType.SupportDocument:
                SupportDocumentsFolder = newDirectoryId;
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

public static class NovelExtensions {
    public static IList<ManuscriptElement> GetScenes(this IList<ManuscriptElement> manuscriptElements) {
        var scenes = new List<ManuscriptElement>();
        foreach (var manuscriptElement in manuscriptElements) {
            if (manuscriptElement.Type == ManuscriptElementType.Scene) {
                scenes.Add(manuscriptElement);
                continue;
            }
            scenes.AddRange(manuscriptElement.ManuscriptElements.GetScenes());
        }

        return scenes;
    }
}
