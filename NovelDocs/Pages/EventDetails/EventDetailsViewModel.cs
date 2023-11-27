using NovelDocs.Entity;
using NovelDocs.PageControls;

namespace NovelDocs.Pages.EventDetails; 

public abstract class EventDetailsViewModel : ViewModel {
    public virtual Event Event { get; set; } = new();

    public virtual string Name {
        get => Event.Name;
        set => Event.Name = value;
    }
}