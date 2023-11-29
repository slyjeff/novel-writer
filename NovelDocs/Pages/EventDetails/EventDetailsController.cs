using System.Linq;
using System.Threading.Tasks;
using NovelDocs.Entity;
using NovelDocs.PageControls;
using NovelDocs.Pages.EventBoard;
using NovelDocs.Services;

namespace NovelDocs.Pages.EventDetails; 

internal class EventDetailsController : Controller<EventDetailsView, EventDetailsViewModel> {
    private readonly IDataPersister _dataPersister;
    private EventViewModel? _eventViewModel;
    private readonly ManuscriptElement _unassignedScene = new() { Name = "(Unassigned)" };

    public EventDetailsController(IDataPersister dataPersister) {
        _dataPersister = dataPersister;
        ViewModel.PropertyChanged += async (_, eventArgs) => {
            if (eventArgs.PropertyName == nameof(ViewModel.Name)) {
                _eventViewModel?. OnPropertyChanged(nameof(EventViewModel.Name));
                await dataPersister.Save();
            }
        };

        ViewModel.AvailableScenes = Novel.ManuscriptElements.GetScenes();
        ViewModel.AvailableScenes.Insert(0, _unassignedScene);
    }

    private Novel Novel => _dataPersister.CurrentNovel;

    public void Initialize(EventViewModel eventViewModel) {
        _eventViewModel = eventViewModel;
        ViewModel.Event = eventViewModel.Event;
        if (ViewModel.Event.SceneId == null) {
            ViewModel.SelectedScene = _unassignedScene;
            return;
        }

        var scene = ViewModel.AvailableScenes.FirstOrDefault(x => x.Id == ViewModel.Event.SceneId);
        if (scene == null) {
            ViewModel.SelectedScene = _unassignedScene;
            return;
        }

        ViewModel.SelectedScene = scene;
    }

    [PropertyChanged]
    public async Task SelectedSceneChanged() {
        if (ViewModel.SelectedScene.Id == ViewModel.Event.SceneId) {
            return;
        }

        if (ViewModel.SelectedScene == _unassignedScene) {
            ViewModel.Event.SceneId = null;
        } else {
            ViewModel.Event.SceneId = ViewModel.SelectedScene.Id;
            ViewModel.Name = ViewModel.SelectedScene.Name;
        }

        await _dataPersister.Save();
    }
}