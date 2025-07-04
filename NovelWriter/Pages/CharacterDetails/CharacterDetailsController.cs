using System;
using System.IO;
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
        var imagesDirectory = DirectoryService.LocalImages;
            
        var extension = Path.GetExtension(path);
        var fileName = Guid.NewGuid() + extension;

        var imagePath = Path.Combine(imagesDirectory, fileName);
        File.Copy(path, imagePath);
        
        //TODO: store image
        //await _googleDocManager.UploadImage(imagePath);

        _treeItem.Character.ImageUriSource = imagePath;
        await _dataPersister.Save();

        _treeItem.OnPropertyChanged(nameof(CharacterTreeItem.ImageUriSource));
        ViewModel.OnPropertyChanged(nameof(ViewModel.ImageUriSource));
    }

    public void Initialize(CharacterTreeItem treeItem) {
        _treeItem = treeItem;

        ViewModel.SetSourceData(treeItem.Character);
        
        _richTextEditorController.Show(ViewModel);
    }
}
