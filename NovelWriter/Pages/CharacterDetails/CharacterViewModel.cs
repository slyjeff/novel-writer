using NovelWriter.Entity;

namespace NovelWriter.Pages.CharacterDetails;

public abstract class CharacterDetailsViewModel : RichTextViewModel<Character> {
    public string ImageUriSource => SourceData.ImageUriSource;
}