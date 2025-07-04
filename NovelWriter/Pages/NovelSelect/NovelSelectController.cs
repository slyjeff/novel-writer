using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NovelWriter.Entity;
using NovelWriter.Extensions;
using NovelWriter.PageControls;
using NovelWriter.Pages.SelectGoogleDriveFolder;
using NovelWriter.Services;

namespace NovelWriter.Pages.NovelSelect; 

internal sealed class NovelSelectController : Controller<NovelSelectView, NovelSelectViewModel> {
    private readonly IDataPersister _dataPersister;
    private readonly IServiceProvider _serviceProvider;
    private Func<Task>? _openNovel;
    
    public NovelSelectController(IDataPersister dataPersister, IServiceProvider serviceProvider) {
        _dataPersister = dataPersister;
        _serviceProvider = serviceProvider;
        ViewModel.Novels = new List<NovelSelectAction> {
            new(null)
        };

        foreach (var novel in dataPersister.Data.Novels.OrderByDescending(x => x.LastModified)) {
            ViewModel.Novels.Add(new NovelSelectAction(novel));
        }
    }

    [Command]
    public async Task ActionSelected(NovelData? novelData) {
        if (_openNovel == null) {
            return;
        }

        if (novelData != null) {
            if (!await _dataPersister.OpenNovel(novelData)) {
                return;
            }
            await _openNovel();
            return;
        }

        var controller = _serviceProvider.CreateInstance<SelectGoogleDriveFolderController>();
        await controller.Initialize();
        if (controller.View.ShowDialog() != true || controller.ViewModel.SelectedDirectory == null) {
            return;
        }

        await _dataPersister.AddNovel(controller.ViewModel.SelectedDirectory);

        await _openNovel();
    }

    public void Initialize(Func<Task> openNovel) {
        _openNovel = openNovel;
    }
}