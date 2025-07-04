using System.Threading.Tasks;
using NovelWriter.Entity;
using NovelWriter.PageControls;
using NovelWriter.Pages.RichTextEditor;
using NovelWriter.Services;

namespace NovelWriter.Pages.NovelDetails; 

// ReSharper disable once ClassNeverInstantiated.Global
internal sealed class NovelDetailsController : Controller<NovelDetailsView, NovelDetailsViewModel> {
    private readonly IDataPersister _dataPersister;

    public NovelDetailsController(IDataPersister dataPersister) {
        _dataPersister = dataPersister;

        ViewModel.PropertyChanged += async (_, _) => {
            await dataPersister.Save();
        };
    }

    public async Task Initialize(Novel novelToLoad) {
        ViewModel.SetNovel(novelToLoad);
        await _dataPersister.Save();
    }
}