using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Media;
using NovelDocs.Entity;
using NovelDocs.PageControls;

namespace NovelDocs.Pages.EventBoard; 

public abstract class EventBoardViewModel : ViewModel {
    public virtual ObservableCollection<HeaderViewModel> Headers { get; set; } = new() {new HeaderViewModel()};
    public virtual ObservableCollection<EventViewModel> Events { get; set; } = new();
    public virtual ObservableCollection<CharacterEventDetailsViewModel> CharacterEventDetails { get; set; } = new();
}

public class HeaderViewModel {
    public HeaderViewModel() {
        Title = "Events";
        ImageUriSource = "/Images/Events.png";
        IsCharacter = false;
    }

    public HeaderViewModel(Character character) {
        Character = character;
        Title = character.Name;
        ImageUriSource = character.ImageUriSource;
        IsCharacter = true;
    }

    public string Title { get; }
    public string ImageUriSource { get; }
    public bool IsCharacter { get; }

    public Character? Character { get; }
}

public class SelectableViewModel : INotifyPropertyChanged {
    public event PropertyChangedEventHandler? PropertyChanged;

    public virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null) {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    private bool _isSelected;
    public bool IsSelected {
        get => _isSelected;
        set {
            _isSelected = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(BorderThickness));
        }
    }

    public int BorderThickness => IsSelected ? 2 : 0;
}

public class EventViewModel : SelectableViewModel {
    private readonly Event _novelEvent;

    public EventViewModel(Event novelEvent) {
        _novelEvent = novelEvent;
    }

    public string Name => _novelEvent.Name;

    public Event Event => _novelEvent;
}

public class CharacterEventDetailsViewModel {
    public CharacterEventDetailsViewModel(Character character) {
        Character = character;
    }

    public Character Character { get; }
    public ObservableCollection<EventDetailsViewModel> EventDetails { get; set; } = new();
}

public class EventDetailsViewModel : SelectableViewModel {
    public EventDetailsViewModel(Event novelEvent) {
        Event = novelEvent;
    }

    public int EventCount { get; set; } = 1;
    public IList<string> Details { get; set; } = new List<string>();
    public SolidColorBrush Background => Details.Any() ? new SolidColorBrush(Colors.Indigo) : new SolidColorBrush((Color)ColorConverter.ConvertFromString("#373737"));

    public Event Event { get; }
}