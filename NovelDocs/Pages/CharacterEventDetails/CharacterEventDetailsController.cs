using NovelDocs.Entity;
using NovelDocs.PageControls;
using NovelDocs.Pages.EventBoard;

namespace NovelDocs.Pages.CharacterEventDetails; 

internal sealed class CharacterEventDetailsController : Controller<CharacterEventDetailsView, CharacterEventDetailsViewModel> {
    public void Initialize(Character character, EventDetailsViewModel eventDetails) {
        ViewModel.Character = character;
        ViewModel.EventDetails = eventDetails;
    }
}