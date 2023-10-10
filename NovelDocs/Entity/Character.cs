using System;

namespace NovelDocs.Entity; 

public sealed class Character {
    public string Name { get; set; } = "New Character";

    public string GoogleDocId { get; set; } = string.Empty;

    public string ImageUriSource { get; set; } = new Random().Next(0, 1) == 1 ? "/images/Character.png" : "/images/Character2.png";
}