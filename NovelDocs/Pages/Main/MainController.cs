using System;
using System.Threading.Tasks;
using NovelDocs.Entity;
using NovelDocs.Extensions;
using NovelDocs.PageControls;
using NovelDocs.Pages.NovelEdit;
using NovelDocs.Pages.NovelSelect;
using NovelDocs.Services;

namespace NovelDocs.Pages.Main; 

internal sealed class MainController : Controller<MainView, MainViewModel> {
    private readonly IDataPersister _dataPersister;
    private readonly IServiceProvider _serviceProvider;

    public MainController(IDataPersister dataPersister, IServiceProvider serviceProvider) {
        _dataPersister = dataPersister;
        _serviceProvider = serviceProvider;
    }

    public async Task Initialize() {
        if (await _dataPersister.OpenNovel()) {
            OpenNovel();
        } else {
            ShowNovelSelector();
        }
    }

    private void OpenNovel() {
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