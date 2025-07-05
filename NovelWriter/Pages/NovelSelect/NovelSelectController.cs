using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NovelWriter.Entity;
using NovelWriter.PageControls;
using NovelWriter.Services;

namespace NovelWriter.Pages.NovelSelect; 

// ReSharper disable once ClassNeverInstantiated.Global
internal sealed class NovelSelectController : Controller<NovelSelectView, NovelSelectViewModel> {
    private readonly IDataPersister _dataPersister;
    private Func<Task>? _openNovel;
    
    public NovelSelectController(IDataPersister dataPersister) {
        _dataPersister = dataPersister;
        ViewModel.Novels = new List<NovelSelectAction> {
            new(null)
        };

        foreach (var novel in dataPersister.AppData.Novels.OrderByDescending(x => x.LastModified)) {
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

        await _dataPersister.AddNovel();

        await _openNovel();
    }

    public void Initialize(Func<Task> openNovel) {
        _openNovel = openNovel;
    }
}
