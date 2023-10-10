using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using NovelDocs.Entity;
using NovelDocs.PageControls;
using NovelDocs.Pages.GoogleDoc;

namespace NovelDocs.Pages.NovelEdit; 

public abstract class NovelEditViewModel : ViewModel {
    protected NovelEditViewModel() {
        Manuscript.ViewModel = this;
        Characters.ViewModel = this;
    }

    public virtual GoogleDocView GoogleDocView { get; set; } = null!; //set in initializer of controller

    public virtual object EditDataView { get; set; } = null!; //set in initialize of controller
    public ManuscriptTreeItem Manuscript => (ManuscriptTreeItem)TreeItems[0];
    public CharactersTreeItem Characters => (CharactersTreeItem)TreeItems[1];

    public IList<object> TreeItems { get; } = new List<object> {
        new ManuscriptTreeItem(),
        new CharactersTreeItem()
    };
}

public interface ITreeItem {
    string Name { get; }

    public bool IsSelected { get; set; }
}

public abstract class TopLevelTreeItem : ITreeItem {
    public event Action? Selected;

    public abstract string Name {get;
}

    private bool _isSelected;
    public bool IsSelected {
        get => _isSelected;
        set {
            _isSelected = value;
            Selected?.Invoke();
        }
    }

   public NovelEditViewModel ViewModel { get; set; } = null!; //set in constructor;
}

public sealed class ManuscriptTreeItem : TopLevelTreeItem {
    public override string Name => "Manuscript";

    public IList<ManuscriptElementTreeItem> ManuscriptElements { get; } = new ObservableCollection<ManuscriptElementTreeItem>();
}

public sealed class CharactersTreeItem : TopLevelTreeItem {
    public override string Name => "Characters";
    public IList<CharacterTreeItem> Characters { get; } = new ObservableCollection<CharacterTreeItem>();
}

public sealed class ManuscriptElementTreeItem : ITreeItem, INotifyPropertyChanged {
    private readonly Action<ManuscriptElementTreeItem> _selected;

    public ManuscriptElementTreeItem(ManuscriptElement manuscriptElement, Action<ManuscriptElementTreeItem> selected) {
        ManuscriptElement = manuscriptElement;
        _selected = selected;
    }

    public ManuscriptElement ManuscriptElement { get; }

    public string Name => ManuscriptElement.Name;

    private bool _isSelected;
    public bool IsSelected {
        get => _isSelected;
        set {
            _isSelected = value;
            _selected(this);
        }
    }

    public string ImageUriSource {
        get {
            return ManuscriptElement.Type switch {
                ManuscriptElementType.Section => "/images/section.png",
                ManuscriptElementType.Paragraph => "/images/paragraph.png",
                ManuscriptElementType.Scene => "/images/scene.png",
                _ => string.Empty
            };
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public void OnPropertyChanged([CallerMemberName] string? propertyName = null) {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}

public sealed class CharacterTreeItem : ITreeItem, INotifyPropertyChanged {
    private readonly Action<CharacterTreeItem> _selected;

    public CharacterTreeItem(Character character, Action<CharacterTreeItem> selected) {
        Character = character;
        _selected = selected;
    }

    public Character Character { get; }

    public string Name => Character.Name;

    private bool _isSelected;
    public bool IsSelected {
        get => _isSelected;
        set {
            _isSelected = value;
            _selected(this);
        }
    }

    public string ImageUriSource => Character.ImageUriSource;

    public event PropertyChangedEventHandler? PropertyChanged;

    public void OnPropertyChanged([CallerMemberName] string? propertyName = null) {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}