using NovelDocs.PageControls;
using NovelDocs.Pages.EventBoard;
using NovelDocs.Services;

namespace NovelDocs.Pages.EventDetails; 

internal class EventDetailsController : Controller<EventDetailsView, EventDetailsViewModel> {
    private EventViewModel? _eventViewModel;

    public EventDetailsController(IDataPersister dataPersister) {
        ViewModel.PropertyChanged += async (sender, eventArgs) => {
            if (eventArgs.PropertyName == nameof(ViewModel.Name)) {
                _eventViewModel?. OnPropertyChanged(nameof(EventViewModel.Name));
                await dataPersister.Save();
            }
        };
    }

    public void Initialize(EventViewModel eventViewModel) {
        _eventViewModel = eventViewModel;
        ViewModel.Event = eventViewModel.Event;
    }
}