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
        new ManuscriptTreeItem { IsExpanded = true },
        new CharactersTreeItem{ IsExpanded = true },
    };
}

public interface INovelTreeItem {
    string Name { get; }

    public bool IsSelected { get; set; }
    public bool IsExpanded { get; set; }
}

public abstract class NovelTreeItem : INovelTreeItem, INotifyPropertyChanged {
    public event Action? Selected;

    public abstract string Name {get;
}

    private bool _isSelected;
    public bool IsSelected {
        get => _isSelected;
        set {
            _isSelected = value;

            OnPropertyChanged();

            if (!_isSelected) {
                return;
            }
            IsExpanded = true;
            Selected?.Invoke();
        }
    }

    private bool _isExpanded;
    public bool IsExpanded {
        get => _isExpanded;
        set {
            if (value == _isExpanded) {
                return;
            }
            _isExpanded = value;
            OnPropertyChanged();
        }
    }

    public NovelEditViewModel ViewModel { get; set; } = null!; //set when created

    public event PropertyChangedEventHandler? PropertyChanged;

    public void OnPropertyChanged([CallerMemberName] string? propertyName = null) {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}

public sealed class ManuscriptTreeItem : NovelTreeItem {
    public override string Name => "Manuscript";

    public IList<ManuscriptElementTreeItem> ManuscriptElements { get; } = new ObservableCollection<ManuscriptElementTreeItem>();
}

public sealed class CharactersTreeItem : NovelTreeItem {
    public override string Name => "Characters";
    public IList<CharacterTreeItem> Characters { get; } = new ObservableCollection<CharacterTreeItem>();
}

public sealed class ManuscriptElementTreeItem : NovelTreeItem {
    public ManuscriptElementTreeItem(ManuscriptElement manuscriptElement, NovelEditViewModel viewModel, Action<ManuscriptElementTreeItem> selected) {
        ManuscriptElement = manuscriptElement;
        ViewModel = viewModel;

        foreach (var childManuscriptElement in manuscriptElement.ManuscriptElements) {
            var newElement = new ManuscriptElementTreeItem(childManuscriptElement, viewModel, selected) {
                Parent = this
            };
            ManuscriptElements.Add(newElement);
        }

        Selected += () => selected(this);

        ManuscriptElements.CollectionChanged += (_, _) => {
            OnPropertyChanged(nameof(CanDelete));
        };
    }

    public ManuscriptElementTreeItem? Parent { get; set; }
    public override string Name => ManuscriptElement.Name;

    public ManuscriptElement ManuscriptElement { get; }

    public ObservableCollection<ManuscriptElementTreeItem> ManuscriptElements { get; } = new();

    public string ImageUriSource {
        get {
            return ManuscriptElement.Type switch {
                ManuscriptElementType.Section => "/images/section.png",
                ManuscriptElementType.Scene => "/images/scene.png",
                _ => "/images/section.png"
            };
        }
    }

    public bool CanAddSection => ManuscriptElement.Type == ManuscriptElementType.Section;
    public bool CanAddScene => ManuscriptElement.Type == ManuscriptElementType.Section;
    public bool CanDelete => ManuscriptElement.ManuscriptElements.Count == 0;
}

public sealed class CharacterTreeItem : NovelTreeItem, INotifyPropertyChanged {
    public CharacterTreeItem(Character character, Action<CharacterTreeItem> selected) {
        Character = character;
        Selected += () => selected(this);
    }

    public Character Character { get; }

    public override string Name => Character.Name;

    public string ImageUriSource => Character.ImageUriSource;
}