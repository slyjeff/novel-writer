using System;
using System.Collections.Generic;

namespace NovelDocs.Entity; 

public sealed class Data {
    public string LastOpenedNovel { get; set; } = string.Empty;
    public IList<NovelData> Novels { get; set; } = new List<NovelData>();
}

public sealed class NovelData {
    public string Name { get; set; } = "New Novel";
    public DateTime LastModified { get; set; } = DateTime.Now;
    public string GoogleId { get; set; } = string.Empty;
}