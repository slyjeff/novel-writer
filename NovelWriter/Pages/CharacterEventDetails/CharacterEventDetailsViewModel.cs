using NovelWriter.Entity;
using NovelWriter.PageControls;
using NovelWriter.Pages.EventBoard;

namespace NovelWriter.Pages.CharacterEventDetails; 

public abstract class CharacterEventDetailsViewModel : ViewModel {
    public virtual Character Character { get; set; } = new();
    public virtual EventDetailsViewModel EventDetails { get; set; } = null!; //this will be set when the controller is initialized
}