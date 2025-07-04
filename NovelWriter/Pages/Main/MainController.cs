using System;
using System.Threading.Tasks;
using NovelWriter.Extensions;
using NovelWriter.Managers;
using NovelWriter.PageControls;
using NovelWriter.Pages.NovelEdit;
using NovelWriter.Pages.NovelSelect;
using NovelWriter.Services;

namespace NovelWriter.Pages.Main; 

internal sealed class MainController : Controller<MainView, MainViewModel> {
    private readonly IDataPersister _dataPersister;
    private readonly IServiceProvider _serviceProvider;
    private readonly IGoogleDocManager _googleDocManager;

    public MainController(IDataPersister dataPersister, IServiceProvider serviceProvider, IGoogleDocManager googleDocManager) {
        _dataPersister = dataPersister;
        _serviceProvider = serviceProvider;
        _googleDocManager = googleDocManager;

        View.Closing += (_, e) => {
            if (!dataPersister.IsSaving) {
                return;
            }

            e.Cancel = true;
            dataPersister.OnFinishedSaving += () => {
                View.Dispatcher.Invoke(() => {
                    View.Close();
                });
            };
        };
    }

    public async Task Initialize() {
        if (await _dataPersister.OpenNovel()) {
            await OpenNovel();
        } else {
            ShowNovelSelector();
        }
    }

    private async Task OpenNovel() {
        await _googleDocManager.DownloadImages();

        var novelEditController = _serviceProvider.CreateInstance<NovelEditController>();
        novelEditController.Initialize(ShowNovelSelector);
        ViewModel.Page = novelEditController.View;
    }

    private void ShowNovelSelector() {
        var novelSelectController = _serviceProvider.CreateInstance<NovelSelectController>();
        novelSelectController.Initialize(OpenNovel);
        ViewModel.Page = novelSelectController.View;
    }
}