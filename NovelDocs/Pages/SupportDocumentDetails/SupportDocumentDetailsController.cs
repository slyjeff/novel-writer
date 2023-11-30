using System.Threading.Tasks;
using NovelDocs.Managers;
using NovelDocs.PageControls;
using NovelDocs.Pages.GoogleDoc;
using NovelDocs.Pages.NovelEdit;
using NovelDocs.Services;

namespace NovelDocs.Pages.SupportDocumentDetails; 

internal sealed class SupportDocumentDetailsController : Controller<SupportDocumentDetailsView, SupportDocumentDetailsViewModel> {
    private readonly IGoogleDocController _googleDocController;
    private SupportDocumentTreeItem _treeItem = null!; //wil be set in the initialize

    public SupportDocumentDetailsController(IDataPersister dataPersister, IGoogleDocController googleDocController, IGoogleDocManager googleDocManager) {
        _googleDocController = googleDocController;
    
        ViewModel.PropertyChanged += async (_, _) => {
            await dataPersister.Save();
            _treeItem.OnPropertyChanged(nameof(SupportDocumentTreeItem.Name));
        };
    }

    public async Task Initialize(SupportDocumentTreeItem treeItem) {
        _treeItem = treeItem;

        ViewModel.SetSourceData(treeItem.SupportDocument);
        
        await _googleDocController.Show(ViewModel);
    }

    [Command]
    public void UnassignGoogleDocId() {
        ViewModel.GoogleDocId = string.Empty;
    }
}