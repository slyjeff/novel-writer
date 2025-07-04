using System.ComponentModel;
using NovelWriter.Entity;
using NovelWriter.PageControls;

namespace NovelWriter;

public enum GoogleDocType { Scene, Character, SupportDocument, Image }

public interface IRichTextViewModel : INotifyPropertyChanged {
    string Name { get; set; }
    string RichText { get; set; }
}

public abstract class RichTextViewModel<T> : ViewModel, IRichTextViewModel where T : class, IDocument, new() {
    public virtual void SetSourceData(T sourceData) {
        SourceData = sourceData;
    }

    protected T SourceData { get; set; } = new();

    public virtual string Name {
        get => SourceData.Name;
        set => SourceData.Name = value;
    }
    
    public virtual string RichText {
        get => SourceData.RichText;
        set => SourceData.RichText = value;
    }
}
