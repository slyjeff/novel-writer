using NovelWriter.Entity;

namespace NovelWriter.Pages.SupportDocumentDetails;

public abstract class SupportDocumentDetailsViewModel : GoogleDocViewModel<SupportDocument> {
    public override GoogleDocType GoogleDocType => GoogleDocType.SupportDocument;
}