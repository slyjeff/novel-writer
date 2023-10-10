using System;
using System.IO;
using System.Threading.Tasks;
using NovelDocs.PageControls;
using NovelDocs.Pages.GoogleDoc;
using NovelDocs.Pages.NovelEdit;
using NovelDocs.Services;

namespace NovelDocs.Pages.SceneDetails; 

internal sealed class SceneDetailsController : Controller<SceneDetailsView, SceneDetailsViewModel> {
    private readonly IGoogleDocController _googleDocController;
    private ManuscriptElementTreeItem _treeItem = null!; //wil be set in the initialize

    public SceneDetailsController(IDataPersister dataPersister, IGoogleDocController googleDocController) {
        _googleDocController = googleDocController;

        ViewModel.PropertyChanged += (_, _) => {
            dataPersister.Save();
            _treeItem.OnPropertyChanged(nameof(CharacterTreeItem.Name));
        };
    }

    public async Task Initialize(ManuscriptElementTreeItem treeItem) {
        _treeItem = treeItem;

        ViewModel.SetSourceData(treeItem.ManuscriptElement);
        
        await _googleDocController.Show(ViewModel);
    }

    [Command]
    public void UnassignGoogleDocId() {
        ViewModel.GoogleDocId = string.Empty;
    }
}