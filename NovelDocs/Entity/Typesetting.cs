namespace NovelDocs.Entity; 

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