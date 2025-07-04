using System.Collections.Generic;
using NovelWriter.Entity;
using NovelWriter.PageControls;

namespace NovelWriter.Pages.EventDetails; 

public abstract class EventDetailsViewModel : ViewModel {
    public virtual Event Event { get; set; } = new();

    public virtual string Name {
        get => Event.Name;
        set => Event.Name = value;
    }

    public virtual IList<ManuscriptElement> AvailableScenes { get; set; } = new List<ManuscriptElement>();
    public virtual ManuscriptElement SelectedScene { get; set; } = null!; //assigned in initialize methods
    public virtual bool CanEditSceneDetails { get; set; }

    public virtual string SceneDetails {
        get => SelectedScene.Summary;
        set => SelectedScene.Summary = value;
    }
}