using System.Collections.Generic;
using System.Linq;
using NovelDocs.Entity;

namespace NovelDocs.Pages.SceneDetails;

public abstract class SceneDetailsViewModel : GoogleDocViewModel<ManuscriptElement> {
    public override GoogleDocType GoogleDocType => GoogleDocType.Scene;

    public IList<Character> AvailableCharacters { get; set; } = new List<Character>() { new Character { Name = "Unassigned", ImageUriSource = "/images/delete.png"} };

    private Character? _pointOfViewCharacter;
    public virtual Character? PointOfViewCharacter {
        get => _pointOfViewCharacter;
        set {
            _pointOfViewCharacter = value;

            if (_pointOfViewCharacter == null) {

            }

            SourceData.PointOfViewCharacterId = _pointOfViewCharacter?.Id;

            OnPropertyChanged(nameof(IsPointOfViewCharacterSelected));
        }
    }

    public bool IsPointOfViewCharacterSelected => PointOfViewCharacter != AvailableCharacters.First();
}
