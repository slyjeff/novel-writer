using System.Collections.Generic;
using NovelDocs.Entity;
using NovelDocs.PageControls;

namespace NovelDocs.Pages.EventDetails; 

public abstract class EventDetailsViewModel : ViewModel {
    public virtual Event Event { get; set; } = new();

    public virtual string Name {
        get => Event.Name;
        set => Event.Name = value;
    }

    public virtual IList<ManuscriptElement> AvailableScenes { get; set; } = new List<ManuscriptElement>();
    public virtual ManuscriptElement SelectedScene { get; set; } = null!; //assigned in initialize methods
}