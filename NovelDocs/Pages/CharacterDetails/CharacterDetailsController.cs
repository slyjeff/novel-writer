using NovelDocs.PageControls;
using NovelDocs.Pages.NovelEdit;
using NovelDocs.Services;
using System;
using System.IO;

namespace NovelDocs.Pages.CharacterDetails; 

internal sealed class CharacterDetailsController : Controller<CharacterDetailsView, CharacterDetailsViewModel> {
    private readonly IDataPersister _dataPersister;
    private CharacterTreeItem _treeItem = null!; //wil be set in the initialize

    public CharacterDetailsController(IDataPersister dataPersister) {
        _dataPersister = dataPersister;
        ViewModel.PropertyChanged += (_, _) => {
            dataPersister.Save();
            _treeItem.OnPropertyChanged(nameof(CharacterTreeItem.Name));
        };

        View.FileDropped += FileDropped;
    }

    private void FileDropped(string path) {
        var directory = AppDomain.CurrentDomain.BaseDirectory;
        var characterImagesDirectory = Path.Combine(directory, "characterImages");
        if (!Directory.Exists(characterImagesDirectory)) {
            Directory.CreateDirectory(characterImagesDirectory);
        }

        var extension = Path.GetExtension(path);
        var fileName = Guid.NewGuid() + "." + extension;

        var imagePath = Path.Combine(characterImagesDirectory, fileName);
        File.Copy(path, imagePath);

        _treeItem.Character.ImageUriSource = imagePath;
        _dataPersister.Save();
        _treeItem.OnPropertyChanged(nameof(CharacterTreeItem.ImageUriSource));
        ViewModel.OnPropertyChanged(nameof(ViewModel.ImageUriSource));
    }

    public void Initialize(CharacterTreeItem treeItem) {
        ViewModel.SetCharacter(treeItem.Character);
        _treeItem = treeItem;
    }
}