using System.Collections.Generic;
using NovelWriter.PageControls;
using NovelWriter.Pages.SceneDetails;

namespace NovelWriter.Pages.SelectCharacter; 

public abstract class SelectCharacterViewModel : ViewModel {
    public virtual List<CharacterWithImage> AvailableCharacters { get; set; } = [];
    public virtual CharacterWithImage? SelectedCharacter { get; set; }
}