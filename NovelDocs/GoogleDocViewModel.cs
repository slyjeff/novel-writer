using System.ComponentModel;
using NovelDocs.Entity;
using NovelDocs.PageControls;

namespace NovelDocs;

public enum GoogleDocType { Scene, Character, Image }

public interface IGoogleDocViewModel : INotifyPropertyChanged {
    string Name { get; set; }
    string GoogleDocId { get; set; }

    GoogleDocType GoogleDocType { get; }
}

public abstract class GoogleDocViewModel<T> : ViewModel, IGoogleDocViewModel where T : class, IGoogleDocItem, new() {
    public virtual void SetSourceData(T sourceData) {
        SourceData = sourceData;
    }

    protected T SourceData { get; private set; } = new T();

    public virtual string Name {
        get => SourceData.Name;
        set => SourceData.Name = value;
    }

    public virtual string GoogleDocId {
        get => SourceData.GoogleDocId;
        set {
            SourceData.GoogleDocId = value;
            OnPropertyChanged(nameof(IsDocumentAssigned));
        }
    }

    public abstract GoogleDocType GoogleDocType { get; }

    public bool IsDocumentAssigned => !string.IsNullOrEmpty(SourceData.GoogleDocId);
}