using System;
using System.Collections.Generic;

namespace NovelDocs.Entity; 

public sealed class Novel {
    public string Name { get; set; } = "New Novel";
    public string Author { get; set; } = string.Empty;
    public string CopyrightYear { get; set; } = DateTime.Today.Year.ToString();
    public string GoogleDriveFolder { get; set; } = string.Empty;
    public string ScenesFolder { get; set; } = string.Empty;
    public string CharactersFolder { get; set; } = string.Empty;
    public DateTime LastModified { get; set; } = DateTime.Now;
    public string ManuscriptId { get; set; } = string.Empty;
    public Typesetting Typesetting { get; set; } = new Typesetting();

    public IList<ManuscriptElement> ManuscriptElements = new List<ManuscriptElement>();

    public IList<Character> Characters = new List<Character>();
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
