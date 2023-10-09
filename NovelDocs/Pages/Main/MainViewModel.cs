using NovelDocs.PageControls;

namespace NovelDocs.Pages.Main;

public abstract class MainViewModel : ViewModel {
    public virtual object Page { get; set; } = null!; //will be set in controller constructor
}