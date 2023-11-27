using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
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

            ViewModel.Headers.Add(new HeaderViewModel(character));
        }
        RefreshEventDetails();
    }

    private EventViewModel CreateEventViewModel(Event novelEvent) {
        var viewModel = new EventViewModel(novelEvent);
        viewModel.PropertyChanged += SelectableViewModelPropertyChanged;
        return viewModel;
    }

    private EventDetailsViewModel CreateEventDetailsViewModel(Event novelEvent) {
        var viewModel = new EventDetailsViewModel(novelEvent);
        viewModel.PropertyChanged += SelectableViewModelPropertyChanged;
        return viewModel;
    }

    private void RefreshEventDetails() {
        var characterHeaders = ViewModel.Headers.Where(x => x.IsCharacter).ToList();
        while (ViewModel.CharacterEventDetails.Count > characterHeaders.Count) {
            ViewModel.CharacterEventDetails.RemoveAt(ViewModel.CharacterEventDetails.Count - 1);
        }

        while (ViewModel.CharacterEventDetails.Count < characterHeaders.Count) {
            var characterHeader = characterHeaders[ViewModel.CharacterEventDetails.Count];
            ViewModel.CharacterEventDetails.Add(new CharacterEventDetailsViewModel(characterHeader.Character!)); //this won't be null because we know it's a character
        }

        foreach (var characterEventDetails in ViewModel.CharacterEventDetails) {
            while (characterEventDetails.EventDetails.Count > ViewModel.Events.Count) {
                var eventDetailsToRemove = characterEventDetails.EventDetails.Last();
                eventDetailsToRemove.PropertyChanged -= SelectableViewModelPropertyChanged;
                characterEventDetails.EventDetails.Remove(eventDetailsToRemove);
            }

            while (characterEventDetails.EventDetails.Count < ViewModel.Events.Count) {
                var novelEvent = ViewModel.Events[characterEventDetails.EventDetails.Count];
                characterEventDetails.EventDetails.Add(CreateEventDetailsViewModel(novelEvent.Event));
            }
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

    [Command]
    public void AddEvent() {
        var newEvent = new Event();
        Novel.Events.Add(newEvent);
        ViewModel.Events.Add(CreateEventViewModel(newEvent));
        
        RefreshEventDetails();
        
        _dataPersister.Save();
    }

    [Command]
    public void DeleteEvent(EventViewModel novelEvent) {
        if (MessageBox.Show($"Remove event {novelEvent.Name}?", "Confirm", MessageBoxButton.YesNo) != MessageBoxResult.Yes) {
            return;
        }

        novelEvent.PropertyChanged -= SelectableViewModelPropertyChanged;

        Novel.Events.Remove(novelEvent.Event);
        ViewModel.Events.Remove(novelEvent);
        
        RefreshEventDetails();

        _dataPersister.Save();
    }

    [Command]
    public void AddCharacter() {
        var selectCharacterController = _serviceProvider.CreateInstance<SelectCharacterController>();
        if (selectCharacterController.View.ShowDialog() != true || selectCharacterController.ViewModel.SelectedCharacter == null) {
            return;
        }

        ViewModel.Headers.Add(new HeaderViewModel(selectCharacterController.ViewModel.SelectedCharacter));

        RefreshEventDetails();

        _dataPersister.Save();
    }

    [Command]
    public void DeleteCharacter(HeaderViewModel headerViewModel) {
        if (headerViewModel.Character == null) {
            return;
        }

        if (MessageBox.Show($"Remove character {headerViewModel.Title}?", "Confirm", MessageBoxButton.YesNo) != MessageBoxResult.Yes) {
            return;
        }

        var eventBoardCharacter = Novel.EventBoardCharacters.FirstOrDefault(x => x.Id == headerViewModel.Character.Id);
        if (eventBoardCharacter == null) {
            return;
        }
        ViewModel.Headers.Remove(headerViewModel);
        Novel.EventBoardCharacters.Remove(eventBoardCharacter);
        RefreshEventDetails();

        _dataPersister.Save();
    }
}