using System;

namespace NovelWriter.Entity; 

public sealed class Event {
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid? SceneId { get; set; }
    public string Name { get; set; } = "New Event";
}