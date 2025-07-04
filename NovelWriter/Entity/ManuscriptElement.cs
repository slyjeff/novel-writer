using System;
using System.Collections.Generic;

namespace NovelWriter.Entity; 

public enum ManuscriptElementType { Section, Scene }

public sealed class ManuscriptElement : IGoogleDocItem {
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;

    public bool IsChapter { get; set; }
    
    public ManuscriptElementType Type { get; set; }

    public string GoogleDocId { get; set; } = string.Empty;

    public IList<ManuscriptElement> ManuscriptElements = new List<ManuscriptElement>();

    public Guid? PointOfViewCharacterId { get; set; }
    public string Summary { get; set; } = string.Empty;
    public IList<PlotPoint> PlotPoints { get; set; } = new List<PlotPoint>();
    public IList<Guid> CharactersInScene { get; set; } = new List<Guid>();
}

public static class ManuscriptElementExtensions {
     public static IList<string> GetDocIds(this IEnumerable<ManuscriptElement> manuscriptElements) {
        var docIds = new List<string>();
        foreach (var manuscriptElement in manuscriptElements) {
            if (manuscriptElement.Type != ManuscriptElementType.Scene) {
                if (manuscriptElement.IsChapter) {
                    docIds.Add($"Chapter:{manuscriptElement.Name}");
                }
                docIds.AddRange(manuscriptElement.ManuscriptElements.GetDocIds());
                continue;
            }

            if (string.IsNullOrEmpty(manuscriptElement.GoogleDocId)) {
                continue;
            }

            docIds.Add(manuscriptElement.GoogleDocId);
        }

        return docIds;
    }
}