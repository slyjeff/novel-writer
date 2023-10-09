using System;
using System.Collections.Generic;
using NovelDocs.Entity;
using NovelDocs.PageControls;
using NovelDocs.Services;

namespace NovelDocs.Pages.NovelSelect; 

internal sealed class NovelSelectController : Controller<NovelSelectView, NovelSelectViewModel> {
    private readonly IDataPersister _dataPersister;
    private Action<Novel>? _openNovel;
    
    public NovelSelectController(IDataPersister dataPersister) {
        _dataPersister = dataPersister;
        ViewModel.Novels = new List<NovelSelectAction> {
            new(null)
        };

        foreach (var novel in dataPersister.Data.Novels) {
            ViewModel.Novels.Add(new NovelSelectAction(novel));
        }
    }

    [Command]
    public void ActionSelected(Novel? novel) {
        if (_openNovel == null) {
            return;
        }

        if (novel == null) {
            novel = new Novel();
            _dataPersister.Data.Novels.Add(novel);
            _dataPersister.Save();
        }

        _openNovel(novel);
    }

    public void Initialize(Action<Novel> openNovel) {
        _openNovel = openNovel;
    }
}