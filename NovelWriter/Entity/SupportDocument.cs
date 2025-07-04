namespace NovelWriter.Entity; 

public sealed class SupportDocument : IDocumentOwner {
    public string Name { get; set; } = string.Empty;
    public int DocumentId { get; set; }
}