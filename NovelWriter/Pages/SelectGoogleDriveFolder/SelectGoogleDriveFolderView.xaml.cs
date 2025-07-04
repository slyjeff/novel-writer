using System;
using System.Windows.Input;

namespace NovelWriter.Pages.SelectGoogleDriveFolder; 

public partial class SelectGoogleDriveFolderView {
    public SelectGoogleDriveFolderView() {
        InitializeComponent();
    }

    public event Action? DirectoryDoubleClicked;

    private void ListView_OnMouseDoubleClick(object sender, MouseButtonEventArgs e) {
        DirectoryDoubleClicked?.Invoke();
    }
}