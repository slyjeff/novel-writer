using System;
using System.Collections.Generic;
using System.Drawing.Text;
using System.Linq;
using System.Windows;
using NovelDocs.PageControls;
using NovelDocs.Services;

namespace NovelDocs.Pages.TypesettingOptions; 

internal class TypesettingOptionsController : Controller<TypesettingOptionsView, TypesettingOptionsViewModel> {

    public TypesettingOptionsController(IDataPersister dataPersister) {
        View.Owner = Application.Current.MainWindow;
        ViewModel.FontSizes = new List<int> { 10, 11, 12, 13, 14 };
        using (var installedFonts = new InstalledFontCollection()) {
            ViewModel.Fonts = installedFonts.Families.Select(f => f.Name).ToList();
        }

        ViewModel.Novel = dataPersister.GetLastOpenedNovel() ?? throw new Exception("Novel not found.");

        ViewModel.PropertyChanged += (_, _) => {
            dataPersister.Save();
        };
    }

    [Command]
    public void TypesetNovel() {
        View.DialogResult = true;
    }
}