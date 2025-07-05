using System;
using System.Windows.Media.Imaging;
using NovelWriter.Entity;
using NovelWriter.PageControls;
using NovelWriter.Pages.EventBoard;
using NovelWriter.Pages.SceneDetails;

namespace NovelWriter.Pages.CharacterEventDetails; 

public abstract class CharacterEventDetailsViewModel : ViewModel {
    public virtual CharacterWithImage Character { get; set; } = new(new Character(), new BitmapImage(new Uri("/images/delete.png", UriKind.Relative)));
    public virtual EventDetailsViewModel EventDetails { get; set; } = null!; //this will be set when the controller is initialized
}
