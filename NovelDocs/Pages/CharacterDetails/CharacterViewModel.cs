using NovelDocs.Entity;

namespace NovelDocs.Pages.CharacterDetails;

public abstract class CharacterDetailsViewModel : GoogleDocViewModel<Character> {
    public string ImageUriSource => SourceData.ImageUriSource;

    public override GoogleDocType GoogleDocType => GoogleDocType.Character;
}