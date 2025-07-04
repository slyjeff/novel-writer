using System.ComponentModel;
using NovelWriter.Entity;
using NovelWriter.PageControls;

namespace NovelWriter;

public enum GoogleDocType { Scene, Character, SupportDocument, Image }

public interface IRichTextViewModel : INotifyPropertyChanged  {
    string Name { get; set; }
    IDocumentOwner DocumentOwner { get; }
}

public abstract class RichTextViewModel<T> : ViewModel, IRichTextViewModel where T : class, IDocumentOwner, new() {
    public virtual void SetSourceData(T sourceData) {
        SourceData = sourceData;
    }

    public T SourceData { get; set; } = new T();

    public virtual string Name {
        get => SourceData.Name;
        set => SourceData.Name = value;
    }
    
    public IDocumentOwner DocumentOwner => SourceData;
}
