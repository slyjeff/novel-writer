using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using NovelDocs.Entity;
using NovelDocs.PageControls;

namespace NovelDocs.Pages.EventBoard; 

public abstract class EventBoardViewModel : ViewModel {
    public ObservableCollection<EventViewModel> Events { get; set; } = new();
    public IList<string> Headers { get; } = new List<string> { "Events" };

    //public virtual IList<CharacterColumnViewModel> CharacterColumns { get; set; } = new List<CharacterColumnViewModel>();
}

public class EventViewModel : INotifyPropertyChanged {
    private readonly Event _novelEvent;

    public EventViewModel(Event novelEvent) {
        _novelEvent = novelEvent;
    }

    public string Name => _novelEvent.Name;

    public Event Event => _novelEvent;

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

    public event PropertyChangedEventHandler? PropertyChanged;

    public virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null) {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}

//public class CharacterColumnViewModel {
//    private readonly EventBoardViewModel _eventBoardViewModel;

//    public CharacterColumnViewModel(string name, EventBoardViewModel eventBoardViewModel, CharacterEventDetails characterEventDetails) {
//        _eventBoardViewModel = eventBoardViewModel;
//        Name = name;
//    }

//    public string Name { get; }
//}