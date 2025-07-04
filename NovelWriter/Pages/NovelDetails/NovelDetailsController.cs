using System.Threading.Tasks;
using NovelWriter.Entity;
using NovelWriter.PageControls;
using NovelWriter.Pages.GoogleDoc;
using NovelWriter.Services;

namespace NovelWriter.Pages.NovelDetails; 

internal sealed class NovelDetailsController : Controller<NovelDetailsView, NovelDetailsViewModel> {
    private readonly IDataPersister _dataPersister;

    public NovelDetailsController(IDataPersister dataPersister, IGoogleDocController googleDocController) {
        _dataPersister = dataPersister;

        googleDocController.Hide();

        ViewModel.PropertyChanged += async (_, _) => {
            await dataPersister.Save();
        };
    }

    public async Task Initialize(Novel novelToLoad) {
        ViewModel.SetNovel(novelToLoad);
        await _dataPersister.Save();
    }
}