using System.Collections.Generic;
using NovelWriter.Entity;
using NovelWriter.PageControls;

namespace NovelWriter.Pages.SelectCharacter; 

public abstract class SelectCharacterViewModel : ViewModel {
    public virtual IList<Character> AvailableCharacters { get; set; } = new List<Character>();
    public virtual Character? SelectedCharacter { get; set; }
}