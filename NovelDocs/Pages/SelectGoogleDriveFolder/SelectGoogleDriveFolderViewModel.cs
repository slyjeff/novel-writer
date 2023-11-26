using System.Collections.Generic;
using NovelDocs.PageControls;
using NovelDocs.Services;

namespace NovelDocs.Pages.SelectGoogleDriveFolder; 

public abstract class SelectGoogleDriveFolderViewModel : ViewModel {
    private IGoogleDirectory? _selectedDirectory;
    public virtual IList<IGoogleDirectory> DirectoryList { get; set; } = new List<IGoogleDirectory>();

    public IGoogleDirectory? SelectedDirectory {
        get => _selectedDirectory;
        set {
            _selectedDirectory = value;
            OnPropertyChanged(nameof(IsDirectorySelected));
        }
    }

    public bool IsDirectorySelected => _selectedDirectory != null;
}