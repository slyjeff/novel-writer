using System;
using System.IO;
using System.Threading.Tasks;
using NovelWriter.Managers;
using NovelWriter.PageControls;
using NovelWriter.Pages.GoogleDoc;
using NovelWriter.Pages.NovelEdit;
using NovelWriter.Services;

namespace NovelWriter.Pages.CharacterDetails; 

internal sealed class CharacterDetailsController : Controller<CharacterDetailsView, CharacterDetailsViewModel> {
    private readonly IDataPersister _dataPersister;
    private readonly IGoogleDocController _googleDocController;
    private readonly IGoogleDocManager _googleDocManager;
    private CharacterTreeItem _treeItem = null!; //wil be set in the initialize

    public CharacterDetailsController(IDataPersister dataPersister, IGoogleDocController googleDocController, IGoogleDocManager googleDocManager) {
        _dataPersister = dataPersister;
        _googleDocController = googleDocController;
        _googleDocManager = googleDocManager;

        ViewModel.PropertyChanged += async (_, _) => {
            await dataPersister.Save();
            _treeItem.OnPropertyChanged(nameof(CharacterTreeItem.Name));
        };

        View.FileDropped += FileDropped;
    }

    private async void FileDropped(string path) {
        var imagesDirectory = DirectoryService.LocalImages;
            
        var extension = Path.GetExtension(path);
        var fileName = Guid.NewGuid() + extension;

        var imagePath = Path.Combine(imagesDirectory, fileName);
        File.Copy(path, imagePath);
        await _googleDocManager.UploadImage(imagePath);

        _treeItem.Character.ImageUriSource = imagePath;
        await _dataPersister.Save();

        _treeItem.OnPropertyChanged(nameof(CharacterTreeItem.ImageUriSource));
        ViewModel.OnPropertyChanged(nameof(ViewModel.ImageUriSource));
    }

    public async Task Initialize(CharacterTreeItem treeItem) {
        _treeItem = treeItem;

        ViewModel.SetSourceData(treeItem.Character);
        
        await _googleDocController.Show(ViewModel);
    }

    [Command]
    public void UnassignGoogleDocId() {
        ViewModel.GoogleDocId = string.Empty;
    }
}