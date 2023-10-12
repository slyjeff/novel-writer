using System;
using System.Collections.Generic;

namespace NovelDocs.Entity; 

public enum ManuscriptElementType { Section, Scene }

public sealed class ManuscriptElement : IGoogleDocItem {
    public string Name { get; set; } = string.Empty;
    
    public ManuscriptElementType Type { get; set; }

    public string GoogleDocId { get; set; } = string.Empty;

    public IList<ManuscriptElement> ManuscriptElements = new List<ManuscriptElement>();

    public Guid? PointOfViewCharacterId { get; set; }
    public string Summary { get; set; } = string.Empty;
    public IList<PlotPoint> PlotPoints { get; set; } = new List<PlotPoint>();
}