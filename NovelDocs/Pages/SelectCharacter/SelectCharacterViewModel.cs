using NovelDocs.Entity;
using NovelDocs.PageControls;
using System.Collections.Generic;

namespace NovelDocs.Pages.SelectCharacter; 

public abstract class SelectCharacterViewModel : ViewModel {
    public virtual IList<Character> AvailableCharacters { get; set; } = new List<Character>();
    public virtual Character? SelectedCharacter { get; set; }
}