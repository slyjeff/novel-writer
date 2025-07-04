using System.Threading.Tasks;
using NovelWriter.PageControls;
using NovelWriter.Pages.GoogleDoc;
using NovelWriter.Pages.NovelEdit;
using NovelWriter.Services;

namespace NovelWriter.Pages.SupportDocumentDetails; 

internal sealed class SupportDocumentDetailsController : Controller<SupportDocumentDetailsView, SupportDocumentDetailsViewModel> {
    private readonly IGoogleDocController _googleDocController;
    private SupportDocumentTreeItem _treeItem = null!; //wil be set in the initialize

    public SupportDocumentDetailsController(IDataPersister dataPersister, IGoogleDocController googleDocController) {
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