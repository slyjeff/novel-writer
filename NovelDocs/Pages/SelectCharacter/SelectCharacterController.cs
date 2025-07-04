using System.Linq;
using System.Windows;
using NovelDocs.PageControls;
using NovelDocs.Services;

namespace NovelDocs.Pages.SelectCharacter; 

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