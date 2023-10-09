using NovelDocs.PageControls;

namespace NovelDocs.Pages.NovelEdit {
    public abstract class NovelEditViewModel : ViewModel {
        public virtual object EditDataView { get; set; } = null!; //set in initialize of controller
    }
}