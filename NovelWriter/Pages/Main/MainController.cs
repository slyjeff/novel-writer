using System;
using System.Threading.Tasks;
using NovelWriter.Extensions;
using NovelWriter.PageControls;
using NovelWriter.Pages.NovelEdit;
using NovelWriter.Pages.NovelSelect;
using NovelWriter.Services;

namespace NovelWriter.Pages.Main; 

// ReSharper disable once ClassNeverInstantiated.Global
internal sealed class MainController(IDataPersister dataPersister, IServiceProvider serviceProvider) : Controller<MainView, MainViewModel> {
    public async Task Initialize() {
        if (await dataPersister.OpenNovel()) {
            await OpenNovel();
        } else {
            ShowNovelSelector();
        }
    }

    private async Task OpenNovel() {
        var novelEditController = serviceProvider.CreateInstance<NovelEditController>();
        await novelEditController.Initialize(ShowNovelSelector);
        ViewModel.Page = novelEditController.View;
    }

    private void ShowNovelSelector() {
        var novelSelectController = serviceProvider.CreateInstance<NovelSelectController>();
        novelSelectController.Initialize(OpenNovel);
        ViewModel.Page = novelSelectController.View;
    }
}