using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using NovelDocs.Entity;
using NovelDocs.Extensions;
using NovelDocs.PageControls;
using NovelDocs.Pages.EventDetails;
using NovelDocs.Services;

namespace NovelDocs.Pages.EventBoard; 

internal sealed class EventBoardController : Controller<EventBoardView, EventBoardViewModel> {
    private readonly IDataPersister _dataPersister;
    private readonly IServiceProvider _serviceProvider;
    private Action<object?>? _showEditDataView;

    public EventBoardController(IDataPersister dataPersister, IServiceProvider serviceProvider) {
        _dataPersister = dataPersister;
        _serviceProvider = serviceProvider;
    }

    private Novel Novel => _dataPersister.CurrentNovel;

    public void Initialize(Action<object?> showEditDataView) {
        _showEditDataView = showEditDataView;
        ViewModel.Events = new ObservableCollection<EventViewModel>(Novel.Events.Select(CreateEventViewModel));
    }

    private EventViewModel CreateEventViewModel(Event novelEvent) {
        var viewModel = new EventViewModel(novelEvent);
        viewModel.PropertyChanged += EventViewModelPropertyChanged;
        return viewModel;
    }

    private bool _changingSelected;

    private void EventViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e) {
        if (e.PropertyName == nameof(EventViewModel.IsSelected)) {
            if (sender is not EventViewModel viewModel) {
                return;
            }

            if (_changingSelected) {
                return;
            }

            if (!viewModel.IsSelected) {
                _showEditDataView?.Invoke(null);
            }

            try {
                _changingSelected = true;
                foreach (var eventViewModel in ViewModel.Events) {
                    if (eventViewModel == sender) {
                        continue;
                    }
                    eventViewModel.IsSelected = false;
                }

                var eventDetailsController = _serviceProvider.CreateInstance<EventDetailsController>();
                eventDetailsController.Initialize(viewModel);
                _showEditDataView?.Invoke(eventDetailsController.View);
            } finally {
                _changingSelected = false;
            }
        }
    }

    [Command]
    public void AddEvent() {
        var newEvent = new Event();
        Novel.Events.Add(newEvent);
        ViewModel.Events.Add(CreateEventViewModel(newEvent));
        _dataPersister.Save();
    }

    [Command]
    public void DeleteEvent(EventViewModel novelEvent) {
        Novel.Events.Remove(novelEvent.Event);
        ViewModel.Events.Remove(novelEvent);
        _dataPersister.Save();
    }
}