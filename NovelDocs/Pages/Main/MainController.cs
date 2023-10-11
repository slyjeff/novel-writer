using System;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using NovelDocs.Entity;
using NovelDocs.Extensions;
using NovelDocs.PageControls;
using NovelDocs.Pages.NovelEdit;
using NovelDocs.Pages.NovelSelect;
using NovelDocs.Services;

namespace NovelDocs.Pages.Main; 

internal sealed class MainController : Controller<MainView, MainViewModel> {
    private readonly IServiceProvider _serviceProvider;

    public MainController(IDataPersister dataPersister, IServiceProvider serviceProvider) {
        _serviceProvider = serviceProvider;

        var novelToOpen = dataPersister.GetLastOpenedNovel();
        if (novelToOpen == null) {
            ShowNovelSelector();
        } else {
            OpenNovel(novelToOpen);
        }
    }

    private void OpenNovel(Novel novel) {
        var novelEditController = _serviceProvider.CreateInstance<NovelEditController>();
        novelEditController.Initialize(ShowNovelSelector, novel);
        ViewModel.Page = novelEditController.View;
    }

    private void ShowNovelSelector() {
        var novelSelectController = _serviceProvider.CreateInstance<NovelSelectController>();
        novelSelectController.Initialize(OpenNovel);
        ViewModel.Page = novelSelectController.View;
    }
}