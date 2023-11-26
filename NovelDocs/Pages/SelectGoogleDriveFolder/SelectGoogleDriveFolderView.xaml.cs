using System;
using System.Windows;
using System.Windows.Input;

namespace NovelDocs.Pages.SelectGoogleDriveFolder; 

public partial class SelectGoogleDriveFolderView {
    public SelectGoogleDriveFolderView() {
        InitializeComponent();
    }

    public event Action? DirectoryDoubleClicked;

    private void ListView_OnMouseDoubleClick(object sender, MouseButtonEventArgs e) {
        DirectoryDoubleClicked?.Invoke();
    }
}