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

    public bool IsPointOfViewCharacterSelected => PointOfViewCharacter != AvailableCharacters.First();

    public IList<CharacterInSceneViewModel> CharactersInScene { get; } = new ObservableCollection<CharacterInSceneViewModel>();
}

public sealed class CharacterInSceneViewModel : INotifyPropertyChanged {
    private readonly Character _removeCharacter = new() { Name = "(Remove)" };

    public CharacterInSceneViewModel(IList<Character> availableCharacters) {
        AvailableCharacters = availableCharacters;
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

    public IList<Character> AvailableCharacters { get; private set; } = new List<Character>();

    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null) {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
