using System.Collections.Generic;

namespace NovelDocs.Entity {
    public sealed class Data {
        public string LastOpenedNovel { get; set; } = string.Empty;
        public IList<Novel> Novels { get; set; } = new List<Novel>();
    }
}