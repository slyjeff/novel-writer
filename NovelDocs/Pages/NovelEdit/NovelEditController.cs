using NovelDocs.Entity;
using NovelDocs.PageControls;
using NovelDocs.Services;

namespace NovelDocs.Pages.NovelEdit {
    internal sealed class NovelEditController : Controller<NovelEditView, NovelEditViewModel> {
        private readonly IDataPersister _dataPersister;

        public NovelEditController(IDataPersister dataPersister) {
            _dataPersister = dataPersister;
            ViewModel.PropertyChanged += (_, _) => {
                dataPersister.Save();
                _dataPersister.Data.LastOpenedNovel = ViewModel.Name;
            };
        }

        public void Initialize(Novel novelToLoad) {
            ViewModel.SetNovel(novelToLoad);
            _dataPersister.Data.LastOpenedNovel = novelToLoad.Name;
            _dataPersister.Save();
        }
    }
}