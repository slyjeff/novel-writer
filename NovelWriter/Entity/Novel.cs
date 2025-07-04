using System;
using System.Collections.Generic;
using System.Linq;

namespace NovelWriter.Entity; 

public sealed class Novel {
    public string Name { get; set; } = "New Novel";
    public string Author { get; set; } = string.Empty;
    public string CopyrightYear { get; set; } = DateTime.Today.Year.ToString();
    public Typesetting Typesetting { get; set; } = new Typesetting();
    
    public List<ManuscriptElement> ManuscriptElements { get; set; } = [];

    public List<Character> Characters { get; set; } = [];

    public List<Event> Events { get; set; } = [];

    public List<EventBoardCharacter> EventBoardCharacters { get; set; } = [];

    public List<SupportDocument> SupportDocuments { get; set; } = [];

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
