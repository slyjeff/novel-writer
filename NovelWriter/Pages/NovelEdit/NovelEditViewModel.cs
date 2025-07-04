using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using NovelWriter.Entity;
using NovelWriter.PageControls;

namespace NovelWriter.Pages.NovelEdit; 

public abstract class NovelEditViewModel : ViewModel {
    protected NovelEditViewModel() {
        Manuscript.ViewModel = this;
        Characters.ViewModel = this;
        SupportDocuments.ViewModel = this;
    }

    public virtual object? ContentView { get; set; } = null;

    public virtual object? EditDataView { get; set; } = null;
    public ManuscriptTreeItem Manuscript => (ManuscriptTreeItem)TreeItems[0];

    public EventBoardTreeItem EventBoard => (EventBoardTreeItem)TreeItems[1];

    public CharactersTreeItem Characters => (CharactersTreeItem)TreeItems[2];

    public SupportDocumentsTreeItem SupportDocuments => (SupportDocumentsTreeItem)TreeItems[3];

    public IList<object> TreeItems { get; } = new List<object> {
        new ManuscriptTreeItem { IsExpanded = false },
        new EventBoardTreeItem(),
        new CharactersTreeItem{ IsExpanded = false },
        new SupportDocumentsTreeItem {IsExpanded = false }
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

public sealed class EventBoardTreeItem : NovelTreeItem {
    public override string Name => "Event Board";
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

public sealed class CharacterTreeItem : NovelTreeItem {
    public CharacterTreeItem(Character character, NovelEditViewModel viewModel, Action<CharacterTreeItem> selected) {
        ViewModel = viewModel;
        Character = character;
        Selected += () => selected(this);
    }

    public Character Character { get; }

    public override string Name => Character.Name;

    public string ImageUriSource => Character.ImageUriSource;
}

public sealed class SupportDocumentsTreeItem : NovelTreeItem {
    public override string Name => "Support Documents";
    public ObservableCollection<SupportDocumentTreeItem> Documents { get; } = new ObservableCollection<SupportDocumentTreeItem>();
}

public sealed class SupportDocumentTreeItem : NovelTreeItem {
    public SupportDocumentTreeItem(SupportDocument supportDocument, NovelEditViewModel viewModel, Action<SupportDocumentTreeItem> selected) {
        ViewModel = viewModel;
        SupportDocument = supportDocument;
        Selected += () => selected(this);
    }

    public SupportDocument SupportDocument { get; }

    public override string Name => SupportDocument.Name;
}
