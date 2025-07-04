using System;
using System.Collections.Generic;

namespace NovelWriter.Entity; 

public sealed class AppData {
    public int Id { get; set; }
    public string LastOpenedNovel { get; set; } = string.Empty;
    public List<NovelData> Novels { get; set;  } = [];
    public int NavigatorWidth { get; set; } = 550;
}

public sealed class NovelData {
    public string Name { get; set; } = "New Novel";
    
    public string FileName => $"{Name.ToLower().Replace(" ", "-")}.nd";

    public DateTime LastModified { get; set; } = DateTime.Now;
}