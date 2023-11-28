using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using NovelDocs.Entity;
using NovelDocs.Extensions;
using NovelDocs.PageControls;
using NovelDocs.Pages.CharacterEventDetails;
using NovelDocs.Pages.EventDetails;
using NovelDocs.Pages.SelectCharacter;
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
        foreach (var eventBoardCharacter in Novel.EventBoardCharacters) {
            var character = Novel.FindCharacterById(eventBoardCharacter.Id);
            if (character == null) {
                continue;
            }

            ViewModel.CharacterHeaders.Add(character);
        }
        RefreshEventDetails();
    }

    private EventViewModel CreateEventViewModel(Event novelEvent) {
        var viewModel = new EventViewModel(novelEvent);
        viewModel.PropertyChanged += SelectableViewModelPropertyChanged;
        return viewModel;
    }

    private EventDetailsViewModel CreateEventDetailsViewModel(Event novelEvent, Character character) {
        var viewModel = new EventDetailsViewModel(novelEvent, character);
        viewModel.PropertyChanged += SelectableViewModelPropertyChanged;
        viewModel.PropertyChanged += EventDetailsViewModelPropertyChanged;
        return viewModel;
    }

    private bool _refreshingEventDetails;
    private void RefreshEventDetails() {
        _refreshingEventDetails = true;
        try {
            while (ViewModel.CharacterEventDetails.Count > ViewModel.CharacterHeaders.Count) {
                ViewModel.CharacterEventDetails.RemoveAt(ViewModel.CharacterEventDetails.Count - 1);
            }

            while (ViewModel.CharacterEventDetails.Count < ViewModel.CharacterHeaders.Count) {
                var character = ViewModel.CharacterHeaders[ViewModel.CharacterEventDetails.Count];
                ViewModel.CharacterEventDetails.Add(new CharacterEventDetailsViewModel(character));
            }

            foreach (var characterEventDetails in ViewModel.CharacterEventDetails) {
                while (characterEventDetails.EventDetails.Count > ViewModel.Events.Count) {
                    var eventDetailsToRemove = characterEventDetails.EventDetails.Last();
                    eventDetailsToRemove.PropertyChanged -= SelectableViewModelPropertyChanged;
                    characterEventDetails.EventDetails.Remove(eventDetailsToRemove);
                }

                while (characterEventDetails.EventDetails.Count < ViewModel.Events.Count) {
                    var novelEvent = ViewModel.Events[characterEventDetails.EventDetails.Count].Event;
                    var eventDetailsViewModel = CreateEventDetailsViewModel(novelEvent, characterEventDetails.Character);
                    characterEventDetails.EventDetails.Add(eventDetailsViewModel);

                    var eventBoardCharacter = Novel.EventBoardCharacters.FirstOrDefault(x => x.Id == characterEventDetails.Character.Id);

                    var eventDetails = eventBoardCharacter?.EventDetails.FirstOrDefault(x => x.EventId == novelEvent.Id);
                    if (eventDetails == null) {
                        continue;
                    }

                    eventDetailsViewModel.Details = eventDetails.Details;
                    eventDetailsViewModel.EventCount = eventDetails.EventCount;
                }
            }
        } finally {
            _refreshingEventDetails = false;
        }
    }

    private bool _changingSelected;

    private void SelectableViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e) {
        if (e.PropertyName != nameof(SelectableViewModel.IsSelected)) {
            return;
        }

        if (sender is not SelectableViewModel viewModel) {
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
                if (eventViewModel != sender) {
                    eventViewModel.IsSelected = false;
                    continue;
                }

                var eventDetailsController = _serviceProvider.CreateInstance<EventDetailsController>();
                eventDetailsController.Initialize(eventViewModel);
                _showEditDataView?.Invoke(eventDetailsController.View);
            }

            foreach (var characterEventDetails in ViewModel.CharacterEventDetails) {
                foreach (var eventDetails in characterEventDetails.EventDetails) {
                    if (eventDetails != sender) {
                        eventDetails.IsSelected = false;
                        continue;
                    }

                    var characterEventDetailsController = _serviceProvider.CreateInstance<CharacterEventDetailsController>();
                    characterEventDetailsController.Initialize(characterEventDetails.Character, eventDetails);
                    _showEditDataView?.Invoke(characterEventDetailsController.View);
                }
            }
        } finally {
            _changingSelected = false;
        }
    }

    private async void EventDetailsViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e) {
        if (_refreshingEventDetails) {
            return;
        }

        if (e.PropertyName != nameof(EventDetailsViewModel.Details) && e.PropertyName != nameof(EventDetailsViewModel.EventCount)) {
            return;
        }

        if (sender is not EventDetailsViewModel viewModel) {
            return;
        }

        var eventBoardCharacter = Novel.EventBoardCharacters.FirstOrDefault(x => x.Id == viewModel.Character.Id);
        if (eventBoardCharacter == null) {
            return;
        }

        var eventDetails = eventBoardCharacter.EventDetails.FirstOrDefault(x => x.EventId == viewModel.Event.Id);
        if (eventDetails == null) {
            eventDetails = new Entity.EventDetails { EventId = viewModel.Event.Id };
            eventBoardCharacter.EventDetails.Add(eventDetails);
        }

        eventDetails.Details = viewModel.Details;
        eventDetails.EventCount =  viewModel.EventCount;

        if (eventDetails.Details == string.Empty) {
            eventBoardCharacter.EventDetails.Remove(eventDetails);
        }

        await _dataPersister.Save();
    }


    [Command]
    public async Task AddEvent() {
        var newEvent = new Event();
        Novel.Events.Add(newEvent);
        ViewModel.Events.Add(CreateEventViewModel(newEvent));
        
        RefreshEventDetails();
        
        await _dataPersister.Save();
    }

    [Command]
    public async Task DeleteEvent(EventViewModel novelEvent) {
        if (MessageBox.Show($"Remove event {novelEvent.Name}?", "Confirm", MessageBoxButton.YesNo) != MessageBoxResult.Yes) {
            return;
        }

        novelEvent.PropertyChanged -= SelectableViewModelPropertyChanged;

        Novel.Events.Remove(novelEvent.Event);
        ViewModel.Events.Remove(novelEvent);

        foreach (var eventBoardCharacter in Novel.EventBoardCharacters) {
            var eventDetailsToRemove = eventBoardCharacter.EventDetails.FirstOrDefault(x => x.EventId == novelEvent.Event.Id);
            if (eventDetailsToRemove == null) {
                continue;
            }

            eventBoardCharacter.EventDetails.Remove(eventDetailsToRemove);
        }
        
        RefreshEventDetails();

        await _dataPersister.Save();
    }

    [Command]
    public async Task AddCharacter() {
        var selectCharacterController = _serviceProvider.CreateInstance<SelectCharacterController>();
        if (selectCharacterController.View.ShowDialog() != true || selectCharacterController.ViewModel.SelectedCharacter == null) {
            return;
        }

        var selectedCharacter = selectCharacterController.ViewModel.SelectedCharacter;
        ViewModel.CharacterHeaders.Add(selectedCharacter);
        Novel.EventBoardCharacters.Add(new EventBoardCharacter { Id = selectedCharacter.Id });

        RefreshEventDetails();

        await _dataPersister.Save();
    }

    [Command]
    public async Task DeleteCharacter(Character character) {
        if (MessageBox.Show($"Remove character {character.Name}?", "Confirm", MessageBoxButton.YesNo) != MessageBoxResult.Yes) {
            return;
        }

        var eventBoardCharacter = Novel.EventBoardCharacters.FirstOrDefault(x => x.Id == character.Id);
        if (eventBoardCharacter == null) {
            return;
        }
        ViewModel.CharacterHeaders.Remove(character);
        Novel.EventBoardCharacters.Remove(eventBoardCharacter);
        RefreshEventDetails();

        await _dataPersister.Save();
    }
}