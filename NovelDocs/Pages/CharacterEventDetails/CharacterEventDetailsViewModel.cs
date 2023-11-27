using NovelDocs.Entity;
using NovelDocs.PageControls;
using NovelDocs.Pages.EventBoard;

namespace NovelDocs.Pages.CharacterEventDetails; 

public abstract class CharacterEventDetailsViewModel : ViewModel {
    public virtual Character Character { get; set; } = new();
    public virtual EventDetailsViewModel EventDetails { get; set; } = null!; //this will be set when the controller is initialized
}