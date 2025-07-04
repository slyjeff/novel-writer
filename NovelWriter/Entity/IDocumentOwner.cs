namespace NovelWriter.Entity; 

public interface IDocumentOwner {
    int DocumentId { get; set; }
    string Name { get; set; }
}
