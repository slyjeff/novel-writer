using NovelDocs.Entity;

namespace NovelDocs.Pages.SupportDocumentDetails;

public abstract class SupportDocumentDetailsViewModel : GoogleDocViewModel<SupportDocument> {
    public override GoogleDocType GoogleDocType => GoogleDocType.Character;
}