using NovelWriter.PageControls;

namespace NovelWriter.Pages.GoogleDoc; 

public abstract class GoogleDocViewModel : ViewModel  {
    public virtual bool IsVisible { get; set; }

    private bool _documentExists;
    public virtual bool DocumentExists {
        get => _documentExists;
        set {
            if (value == _documentExists) {
                return;
            }
            _documentExists = value;
            OnPropertyChanged(nameof(CanCreateOrLinkDocument));
        }
    }

    private bool _assigningExistingDocument;
    public virtual bool AssigningExistingDocument {
        get => _assigningExistingDocument;
        set {
            GoogleDocId = string.Empty;
            if (value == _assigningExistingDocument) {
                return;
            }
            _assigningExistingDocument = value;
            OnPropertyChanged(nameof(CanCreateOrLinkDocument));
        }
    }

    public bool CanCreateOrLinkDocument => !DocumentExists && !AssigningExistingDocument;

    public virtual string GoogleDocId { get; set; } = string.Empty;
}