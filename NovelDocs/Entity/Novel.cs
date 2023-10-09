using System;

namespace NovelDocs.Entity; 

public sealed class Novel {
    public string Name { get; set; } = "New Novel";
    public string Author { get; set; } = string.Empty;
    public string CopyrightYear { get; set; } = DateTime.Today.Year.ToString();
    public string GoogleDriveFolder { get; set; } = string.Empty;
    public DateTime LastModified { get; set; } = DateTime.Now;
}