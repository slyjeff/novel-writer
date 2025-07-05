using System.Threading.Tasks;
using NovelWriter.PageControls;
using NovelWriter.Pages.EventBoard;
using NovelWriter.Pages.SceneDetails;
using NovelWriter.Services;

namespace NovelWriter.Pages.CharacterEventDetails; 

internal sealed class CharacterEventDetailsController : Controller<CharacterEventDetailsView, CharacterEventDetailsViewModel> {
    private readonly IDataPersister _dataPersister;

    public CharacterEventDetailsController(IDataPersister dataPersister) {
        _dataPersister = dataPersister;
    }
    
    public async Task Initialize(CharacterWithImage character, EventDetailsViewModel eventDetails) {
        var image = await _dataPersister.GetImage(character.Character, 330);
        
        ViewModel.Character = new CharacterWithImage(character.Character, image);
        ViewModel.EventDetails = eventDetails;
    }
}
