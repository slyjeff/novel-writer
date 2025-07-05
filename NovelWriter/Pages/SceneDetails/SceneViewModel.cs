using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Media.Imaging;
using NovelWriter.Entity;

namespace NovelWriter.Pages.SceneDetails;

public abstract class SceneDetailsViewModel : RichTextViewModel<ManuscriptElement> {
    public List<CharacterWithImage> AvailableCharacters { get; set; } = [
        new(
            new Character {
                Name = "Unassigned",
            },
            new BitmapImage(new Uri("/images/delete.png", UriKind.Relative))
        )
    ];

    private CharacterWithImage? _pointOfViewCharacter;
    public virtual CharacterWithImage? PointOfViewCharacter {
        get => _pointOfViewCharacter;
        set {
            _pointOfViewCharacter = value;

            SourceData.PointOfViewCharacterId = _pointOfViewCharacter?.Character.Id;

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

public class CharacterWithImage(Character character, BitmapImage image) {
    public Character Character => character;
    public BitmapImage Image => image;
}

public sealed class CharacterInSceneViewModel : INotifyPropertyChanged {
    private readonly CharacterWithImage _removeCharacter = new(new Character{ Name = "(Remove)" }, new BitmapImage(new Uri("/images/delete.png", UriKind.Relative)));

    public CharacterInSceneViewModel(IEnumerable<CharacterWithImage> availableCharacters) {
        AvailableCharacters = availableCharacters.ToList();
        AvailableCharacters.Insert(0, _removeCharacter);
        SelectedCharacter = AvailableCharacters[1]; //unassigned to start with
    }

    public event Action<CharacterInSceneViewModel>? CharacterRemoved;

    private CharacterWithImage? _selectedCharacter;
    public CharacterWithImage? SelectedCharacter {
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

    public IList<CharacterWithImage> AvailableCharacters { get; private set; }

    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null) {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
