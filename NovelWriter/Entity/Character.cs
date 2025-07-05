using System;

namespace NovelWriter.Entity;

public sealed class Character : IDocumentOwner, IImageOwner {
    public Guid Id { get; set; } = Guid.NewGuid(); 

    public string Name { get; set; } = "New Character";
    public int DocumentId { get; set; }
    public Guid ImageId { get; set; } = Guid.Empty;
}
