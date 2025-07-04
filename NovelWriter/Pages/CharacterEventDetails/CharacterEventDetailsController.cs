using NovelWriter.Entity;
using NovelWriter.PageControls;
using NovelWriter.Pages.EventBoard;

namespace NovelWriter.Pages.CharacterEventDetails; 

internal sealed class CharacterEventDetailsController : Controller<CharacterEventDetailsView, CharacterEventDetailsViewModel> {
    public void Initialize(Character character, EventDetailsViewModel eventDetails) {
        ViewModel.Character = character;
        ViewModel.EventDetails = eventDetails;
    }
}