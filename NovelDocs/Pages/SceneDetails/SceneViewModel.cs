using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using NovelDocs.Entity;

namespace NovelDocs.Pages.SceneDetails;

public abstract class SceneDetailsViewModel : GoogleDocViewModel<ManuscriptElement> {
    public override GoogleDocType GoogleDocType => GoogleDocType.Scene;

    public IList<Character> AvailableCharacters { get; set; } = new List<Character>() { new() { Name = "Unassigned", ImageUriSource = "/images/delete.png"} };

    private Character? _pointOfViewCharacter;
    public virtual Character? PointOfViewCharacter {
        get => _pointOfViewCharacter;
        set {
            _pointOfViewCharacter = value;

            SourceData.PointOfViewCharacterId = _pointOfViewCharacter?.Id;

            OnPropertyChanged(nameof(IsPointOfViewCharacterSelected));
        }
    }

    public virtual string Details {
        get => SourceData.Summary;
        set => SourceData.Summary = value;
    }

    public bool IsPointOfViewCharacterSelected => PointOfViewCharacter != AvailableCharacters.First();

    public IList<CharacterInSceneViewModel> CharactersInScene { get; } = new ObservableCollection<CharacterInSceneViewModel>();
}

public sealed class CharacterInSceneViewModel : INotifyPropertyChanged {
    private readonly Character _removeCharacter = new() { Name = "(Remove)" };

    public CharacterInSceneViewModel(IEnumerable<Character> availableCharacters) {
        AvailableCharacters = availableCharacters.ToList();
        AvailableCharacters.Insert(0, _removeCharacter);
    }

    public event Action<CharacterInSceneViewModel>? CharacterRemoved;

    private Character? _selectedCharacter;
    public Character? SelectedCharacter {
        get => _selectedCharacter;
        set {
            _selectedCharacter = value;

            if (_selectedCharacter == _removeCharacter) {
                CharacterRemoved?.Invoke(this);
            }

            OnPropertyChanged();
            OnPropertyChanged(nameof(IsCharacterSelected));
        }
    }

    public bool IsCharacterSelected => SelectedCharacter != null && SelectedCharacter != _removeCharacter;

    public IList<Character> AvailableCharacters { get; private set; }

    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null) {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
