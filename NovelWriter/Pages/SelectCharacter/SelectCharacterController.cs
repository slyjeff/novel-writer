using System.Linq;
using System.Windows;
using NovelWriter.PageControls;
using NovelWriter.Services;

namespace NovelWriter.Pages.SelectCharacter; 

internal sealed class SelectCharacterController : Controller<SelectCharacterView, SelectCharacterViewModel> {
    public SelectCharacterController(IDataPersister dataPersister) {
        View.Owner = Application.Current.MainWindow;

        var novel = dataPersister.CurrentNovel;
        ViewModel.AvailableCharacters = novel.Characters;
        ViewModel.SelectedCharacter = novel.Characters.FirstOrDefault();
    }

    [Command]
    public void Ok() {
        View.DialogResult = true;
    }
}