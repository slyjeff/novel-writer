using System.Collections.Generic;
using System.Windows.Markup;

namespace NovelDocs.Entity; 

public sealed class Data {
    public string LastOpenedNovel { get; set; } = string.Empty;
    public IList<Novel> Novels { get; set; } = new List<Novel>();
}