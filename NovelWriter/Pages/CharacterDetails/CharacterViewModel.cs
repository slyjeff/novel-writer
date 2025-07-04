using NovelWriter.Entity;

namespace NovelWriter.Pages.CharacterDetails;

public abstract class CharacterDetailsViewModel : GoogleDocViewModel<Character> {
    public string ImageUriSource => SourceData.ImageUriSource;

    public override GoogleDocType GoogleDocType => GoogleDocType.Character;
}