using System;

namespace NovelDocs.Entity; 

public sealed class CharacterEventDetails {
    public Guid EventId { get; set; }
    public Guid LastEventId { get; set; }
    public Guid CharacterId { get; set; }
    public string Details { get; set; } = string.Empty;
}