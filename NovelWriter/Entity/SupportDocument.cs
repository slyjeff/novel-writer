namespace NovelWriter.Entity; 

public sealed class SupportDocument : IDocument {
    public string Name { get; set; } = string.Empty;
    public string GoogleDocId { get; set; } = string.Empty;
    public string RichText { get; set; } = string.Empty;
}