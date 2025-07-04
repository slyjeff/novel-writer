using System;
using System.Collections.Generic;

namespace NovelWriter.Entity; 

public enum ManuscriptElementType { Section, Scene }

public sealed class ManuscriptElement : IDocumentOwner {
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public int DocumentId { get; set; }

    public bool IsChapter { get; set; }
    
    public ManuscriptElementType Type { get; set; }

    public string GoogleDocId { get; set; } = string.Empty;

    public List<ManuscriptElement> ManuscriptElements { get; set; } = [];

    public Guid? PointOfViewCharacterId { get; set; }
    public string Summary { get; set; } = string.Empty;
    public List<PlotPoint> PlotPoints { get; set; } = [];
    public List<Guid> CharactersInScene { get; set; } = [];
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