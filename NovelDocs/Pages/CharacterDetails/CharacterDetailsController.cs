using NovelDocs.PageControls;
using NovelDocs.Pages.NovelEdit;
using NovelDocs.Services;
using System;
using System.IO;
using System.Threading.Tasks;
using NovelDocs.Pages.GoogleDoc;

namespace NovelDocs.Pages.CharacterDetails; 

internal sealed class CharacterDetailsController : Controller<CharacterDetailsView, CharacterDetailsViewModel> {
    private readonly IDataPersister _dataPersister;
    private readonly IGoogleDocController _googleDocController;
    private CharacterTreeItem _treeItem = null!; //wil be set in the initialize

    public CharacterDetailsController(IDataPersister dataPersister, IGoogleDocController googleDocController) {
        _dataPersister = dataPersister;
        _googleDocController = googleDocController;

        ViewModel.PropertyChanged += (_, _) => {
            dataPersister.Save();
            _treeItem.OnPropertyChanged(nameof(CharacterTreeItem.Name));
        };

        View.FileDropped += FileDropped;
    }

    private void AssignDocument(string googleDocId) {
        _treeItem.Character.GoogleDocId = googleDocId;
        _dataPersister.Save();
        ViewModel.OnPropertyChanged(nameof(ViewModel.IsDocumentAssigned));
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

    public async Task Initialize(CharacterTreeItem treeItem) {
        _treeItem = treeItem;

        ViewModel.SetCharacter(treeItem.Character);
        
        await _googleDocController.Show(treeItem.Character.GoogleDocId, AssignDocument);
    }

    [Command]
    public async Task UnassignGoogleDocId() {
        _treeItem.Character.GoogleDocId = string.Empty;
        _dataPersister.Save();
        await _googleDocController.Show(string.Empty, AssignDocument);
        ViewModel.OnPropertyChanged(nameof(ViewModel.IsDocumentAssigned));
    }
}