using System.Collections.Generic;
using System.Drawing.Text;
using System.Linq;
using System.Windows;
using NovelWriter.PageControls;
using NovelWriter.Services;

namespace NovelWriter.Pages.TypesettingOptions; 

internal class TypesettingOptionsController : Controller<TypesettingOptionsView, TypesettingOptionsViewModel> {

    public TypesettingOptionsController(IDataPersister dataPersister) {
        View.Owner = Application.Current.MainWindow;
        ViewModel.FontSizes = new List<int> { 10, 11, 12, 13, 14 };
        using (var installedFonts = new InstalledFontCollection()) {
            ViewModel.Fonts = installedFonts.Families.Select(f => f.Name).ToList();
        }

        ViewModel.Novel = dataPersister.CurrentNovel;

        ViewModel.PropertyChanged += async (_, _) => {
            await dataPersister.Save();
        };
    }

    [Command]
    public void TypesetNovel() {
        View.DialogResult = true;
    }
}