using System;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using NovelWriter.PageControls;
using NovelWriter.Pages.NovelEdit;
using NovelWriter.Pages.RichTextEditor;
using NovelWriter.Services;

namespace NovelWriter.Pages.CharacterDetails; 

// ReSharper disable once ClassNeverInstantiated.Global
internal sealed class CharacterDetailsController : Controller<CharacterDetailsView, CharacterDetailsViewModel> {
    private readonly IDataPersister _dataPersister;
    private readonly IRichTextEditorController _richTextEditorController;
    private CharacterTreeItem _treeItem = null!; //wil be set in the initialize

    public CharacterDetailsController(IDataPersister dataPersister, IRichTextEditorController richTextEditorController) {
        _dataPersister = dataPersister;
        _richTextEditorController = richTextEditorController;

        ViewModel.PropertyChanged += async (_, _) => {
            await dataPersister.Save();
            _treeItem.OnPropertyChanged(nameof(CharacterTreeItem.Name));
        };

        View.FileDropped += FileDropped;
    }

    private async void FileDropped(string path) {
        ViewModel.Image = LoadBitmapFromFile(path, 330);
        _treeItem.Image = LoadBitmapFromFile(path, 50);
        _treeItem.OnPropertyChanged(nameof(ViewModel.Image));

        var imageToStore = LoadBitmapFromFile(path);
        await _dataPersister.SaveImage(_treeItem.Character, imageToStore);
    }

    private BitmapImage LoadBitmapFromFile(string path, int? decodePixelWidth = null) {
        if (decodePixelWidth == null) {
            return new BitmapImage(new Uri(path, UriKind.Absolute));
        }

        var image = new BitmapImage();
        image.BeginInit();
        image.UriSource = new Uri(path, UriKind.Absolute);
        image.DecodePixelWidth = decodePixelWidth.Value;
        image.DecodePixelHeight = decodePixelWidth.Value;
        image.CacheOption = BitmapCacheOption.OnLoad;
        image.EndInit();
        image.Freeze(); // For cross-thread operations
        ViewModel.Image = image;

        return image;
    }

    public async Task Initialize(CharacterTreeItem treeItem) {
        _treeItem = treeItem;

        ViewModel.SetSourceData(treeItem.Character);
        
        ViewModel.Image = await _dataPersister.GetImage(treeItem.Character, 330);
        
        await _richTextEditorController.Show(ViewModel);
    }
}
