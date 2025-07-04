namespace NovelWriter.Entity; 

public sealed class SupportDocument : IGoogleDocItem {
    public string Name { get; set; } = string.Empty;
    public string GoogleDocId { get; set; } = string.Empty;
}