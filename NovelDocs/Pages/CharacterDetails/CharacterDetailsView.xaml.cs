using System;
using System.Windows;

namespace NovelDocs.Pages.CharacterDetails; 

public partial class CharacterDetailsView {
    public CharacterDetailsView() {
        InitializeComponent();
    }

    public event Action<string>? FileDropped;

    private void UIElement_OnDrop(object sender, DragEventArgs e) {
        if (!e.Data.GetDataPresent(DataFormats.FileDrop)) {
            return;
        }
        // Note that you can have more than one file.
        var files = (string[])e.Data.GetData(DataFormats.FileDrop)!;

        FileDropped?.Invoke(files[0]);
    }
}