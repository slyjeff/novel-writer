using System.Threading.Tasks;
using System.Windows;
using NovelWriter.PageControls;
using NovelWriter.Services;

namespace NovelWriter.Pages.SelectGoogleDriveFolder; 

internal sealed class SelectGoogleDriveFolderController : Controller<SelectGoogleDriveFolderView, SelectGoogleDriveFolderViewModel> {
    private readonly IGoogleDocService _googleDocService;

    public SelectGoogleDriveFolderController(IGoogleDocService googleDocService) {
        _googleDocService = googleDocService;
        View.Owner = Application.Current.MainWindow;
        View.DirectoryDoubleClicked += GetSubdirectory;
    }

    private async void GetSubdirectory() {
        if (ViewModel.SelectedDirectory == null) {
            return;
        }

        ViewModel.DirectoryList = await _googleDocService.GetDirectoryList(ViewModel.SelectedDirectory.Id);
        ViewModel.SelectedDirectory = null;
    }

    public async Task Initialize() {
        ViewModel.DirectoryList = await _googleDocService.GetDirectoryList();
    }

    [Command]
    public void SelectDirectory() {
        View.DialogResult = true;
    }
}