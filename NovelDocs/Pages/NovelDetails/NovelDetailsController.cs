using NovelDocs.Entity;
using NovelDocs.PageControls;
using NovelDocs.Services;

namespace NovelDocs.Pages.NovelDetails; 

internal sealed class NovelDetailsController : Controller<NovelDetailsView, NovelDetailsViewModel> {
    private readonly IDataPersister _dataPersister;

    public NovelDetailsController(IDataPersister dataPersister) {
        _dataPersister = dataPersister;
        ViewModel.PropertyChanged += (_, _) => {
            _dataPersister.Data.LastOpenedNovel = ViewModel.Name;
            dataPersister.Save();
        };
    }

    public void Initialize(Novel novelToLoad) {
        ViewModel.SetNovel(novelToLoad);
        _dataPersister.Data.LastOpenedNovel = novelToLoad.Name;
        _dataPersister.Save();
    }
}