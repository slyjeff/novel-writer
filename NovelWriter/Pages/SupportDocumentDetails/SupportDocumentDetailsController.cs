using System.Threading.Tasks;
using NovelWriter.PageControls;
using NovelWriter.Pages.NovelEdit;
using NovelWriter.Pages.RichTextEditor;
using NovelWriter.Services;
// ReSharper disable ClassNeverInstantiated.Global

namespace NovelWriter.Pages.SupportDocumentDetails; 

internal sealed class SupportDocumentDetailsController : Controller<SupportDocumentDetailsView, SupportDocumentDetailsViewModel> {
    private readonly IRichTextEditorController _richTextEditorController;
    private SupportDocumentTreeItem _treeItem = null!; //wil be set in the initialize

    public SupportDocumentDetailsController(IDataPersister dataPersister, IRichTextEditorController richTextEditorController) {
        _richTextEditorController = richTextEditorController;
    
        ViewModel.PropertyChanged += async (_, _) => {
            await dataPersister.Save();
            _treeItem.OnPropertyChanged(nameof(SupportDocumentTreeItem.Name));
        };
    }

    public void Initialize(SupportDocumentTreeItem treeItem) {
        _treeItem = treeItem;

        ViewModel.SetSourceData(treeItem.SupportDocument);
        
        _richTextEditorController.Show(ViewModel);
    }
}
