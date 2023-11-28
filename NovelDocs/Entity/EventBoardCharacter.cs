using System;
using System.Collections.Generic;

namespace NovelDocs.Entity; 

public sealed class EventBoardCharacter {
    public Guid Id { get; set; }

    public IList<EventDetails> EventDetails { get; set; } = new List<EventDetails>();

}

public sealed class EventDetails {
    public Guid EventId { get; set; }
    public int EventCount { get; set; } = 1;
    public string Details { get; set; } = string.Empty;
}