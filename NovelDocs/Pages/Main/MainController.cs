using System;
using System.Linq;
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
        var data = dataPersister.Data;

        var novelToOpen = data.Novels.FirstOrDefault(x => x.Name == data.LastOpenedNovel);
        if (novelToOpen == null) {
            var novelSelectController = serviceProvider.CreateInstance<NovelSelectController>();
            novelSelectController.Initialize(OpenNovel);
            ViewModel.Page = novelSelectController.View;
        } else {
            OpenNovel(novelToOpen);
        }
    }

    private void OpenNovel(Novel novel) {
        var novelEditController = _serviceProvider.CreateInstance<NovelEditController>();
        novelEditController.Initialize(novel);
        ViewModel.Page = novelEditController.View;
    }
}