using System;

namespace NovelWriter.Entity;

public class Document {
    public int Id { get; set; }
    public string Content { get; set; } = string.Empty;
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    public DateTime ModifiedDate { get; set; } = DateTime.UtcNow; 
}

public class DocumentVersion {
    public int DocumentId { get; set; }
    public int Version { get; set; } = 1;
    public DateTime VersionDate { get; set; } = DateTime.UtcNow;
    public string Content { get; set; } = string.Empty;
}
