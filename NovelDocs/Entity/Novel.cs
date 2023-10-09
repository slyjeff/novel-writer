using System;
using System.Collections.Generic;

namespace NovelDocs.Entity; 

public sealed class Novel {
    public string Name { get; set; } = "New Novel";
    public string Author { get; set; } = string.Empty;
    public string CopyrightYear { get; set; } = DateTime.Today.Year.ToString();
    public string GoogleDriveFolder { get; set; } = string.Empty;
    public DateTime LastModified { get; set; } = DateTime.Now;

    public IList<ManuscriptElement> ManuscriptElements = new List<ManuscriptElement>();

    public IList<Character> Characters = new List<Character>();
}