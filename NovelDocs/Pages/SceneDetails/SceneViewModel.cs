using NovelDocs.Entity;

namespace NovelDocs.Pages.SceneDetails;

public abstract class SceneDetailsViewModel : GoogleDocViewModel<ManuscriptElement> {
    public override GoogleDocType GoogleDocType => GoogleDocType.Scene;
}
