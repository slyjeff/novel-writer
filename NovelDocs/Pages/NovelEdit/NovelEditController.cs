using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using Microsoft.Office.Interop.Word;
using Microsoft.Win32;
using NovelDocs.Entity;
using NovelDocs.Extensions;
using NovelDocs.Managers;
using NovelDocs.PageControls;
using NovelDocs.Pages.CharacterDetails;
using NovelDocs.Pages.EventBoard;
using NovelDocs.Pages.GoogleDoc;
using NovelDocs.Pages.NovelDetails;
using NovelDocs.Pages.SceneDetails;
using NovelDocs.Pages.SectionDetails;
using NovelDocs.Pages.SupportDocumentDetails;
using NovelDocs.Pages.TypesettingOptions;
using NovelDocs.Services;
using Task = System.Threading.Tasks.Task;

namespace NovelDocs.Pages.NovelEdit; 

internal sealed class NovelEditController : Controller<NovelEditView, NovelEditViewModel> {
    private readonly IServiceProvider _serviceProvider;
    private readonly IDataPersister _dataPersister;
    private readonly IGoogleDocController _googleDocController;
    private readonly IGoogleDocManager _googleDocManager;
    private readonly IMsWordManager _msWordManager;
    private Action _novelClosed = null!; //will never be null because initialize will always be called

    public NovelEditController(IServiceProvider serviceProvider, IDataPersister dataPersister, IGoogleDocController googleDocController, IGoogleDocManager googleDocManager, IMsWordManager msWordManager) {
        _serviceProvider = serviceProvider;
        _dataPersister = dataPersister;
        _googleDocController = googleDocController;
        _googleDocManager = googleDocManager;
        _msWordManager = msWordManager;

        View.OnMoveNovelTreeItem += MoveNovelTreeItem;
    }

    private Novel Novel => _dataPersister.CurrentNovel;

    public void Initialize(Action novelClosed) {
        _novelClosed = novelClosed;

        ViewModel.Manuscript.Selected += ManuscriptSelected;
        ViewModel.EventBoard.Selected += EventBoardSelected;
        ViewModel.Characters.Selected += CharactersSelected;

        foreach (var element in Novel.ManuscriptElements) {
            var treeItem = new ManuscriptElementTreeItem(element, ViewModel, ManuscriptElementSelected);
            ViewModel.Manuscript.ManuscriptElements.Add(treeItem);
        }

        ViewModel.Manuscript.IsSelected = true;

        foreach (var character in Novel.Characters) {
            var treeItem = new CharacterTreeItem(character, CharacterSelected);
            ViewModel.Characters.Characters.Add(treeItem);
        }

        foreach (var supportDocument in Novel.SupportDocuments) {
            var treeItem = new SupportDocumentTreeItem(supportDocument, SupportDocumentSelected);
            ViewModel.SupportDocuments.Documents.Add(treeItem);
        }
    }

    private async void MoveNovelTreeItem(NovelTreeItem itemToMove, MoveType moveType, NovelTreeItem destinationItem) {
        switch (itemToMove) {
            case CharacterTreeItem characterToMove:
                await MoveCharacter(characterToMove, destinationItem as CharacterTreeItem);
                break;
            case ManuscriptElementTreeItem manuscriptItemToMove:
                await MoveManuscriptElement(manuscriptItemToMove, moveType, destinationItem as ManuscriptElementTreeItem);
                break;
        }
    }

    private async Task MoveCharacter(CharacterTreeItem treeItemToMove, CharacterTreeItem? destinationTreeItem) {
        if (destinationTreeItem == null) {
            return;
        }

        Novel.Characters.Move(treeItemToMove.Character, destinationTreeItem.Character);
        ViewModel.Characters.Characters.Move(treeItemToMove, destinationTreeItem);

        await _dataPersister.Save();

        treeItemToMove.IsSelected = true;
    }

    private async Task MoveManuscriptElement(ManuscriptElementTreeItem treeItemToMove, MoveType moveDestination, ManuscriptElementTreeItem? destinationTreeItem) {
        if (destinationTreeItem == null) {
            await MoveManuscriptElementToRoot(treeItemToMove);
        } else if (moveDestination == MoveType.Into) {
            await MoveManuscriptElementIntoSection(treeItemToMove, destinationTreeItem);
        } else {
            await MoveManuscriptElementBeforeAnotherManuscriptElement(treeItemToMove, destinationTreeItem);
        }
    }

    private async Task MoveManuscriptElementBeforeAnotherManuscriptElement(ManuscriptElementTreeItem treeItemToMove, ManuscriptElementTreeItem destinationTreeItem) {
        var destinationParentList = Novel.ManuscriptElements.FindParentManuscriptElementList(destinationTreeItem.ManuscriptElement);
        if (destinationParentList == null) {
            return;
        }

        //first move the item to the new list, then move it to before the existing item
        Novel.ManuscriptElements.MoveManuscriptElementToList(treeItemToMove.ManuscriptElement, destinationParentList);
        destinationParentList.Move(treeItemToMove.ManuscriptElement, destinationTreeItem.ManuscriptElement);

        var treeItemDestinationParentList = destinationTreeItem.Parent?.ManuscriptElements ?? ViewModel.Manuscript.ManuscriptElements;
        ViewModel.Manuscript.ManuscriptElements.MoveManuscriptElementTreeItemToList(treeItemToMove, treeItemDestinationParentList);
        treeItemDestinationParentList.Move(treeItemToMove, destinationTreeItem);

        treeItemToMove.Parent = destinationTreeItem.Parent;

        await _dataPersister.Save();

        treeItemToMove.IsSelected = true;
        if (treeItemToMove.Parent != null) {
            treeItemToMove.Parent.IsExpanded = true;
        }
    }


    private async Task MoveManuscriptElementToRoot(ManuscriptElementTreeItem treeItemToMove) {
        //this item can't be at the root already
        if (treeItemToMove.Parent == null) {
            return;
        }

        Novel.ManuscriptElements.MoveManuscriptElementToList(treeItemToMove.ManuscriptElement, Novel.ManuscriptElements);

        treeItemToMove.Parent.ManuscriptElements.Remove(treeItemToMove);
        ViewModel.Manuscript.ManuscriptElements.Add(treeItemToMove);
        treeItemToMove.Parent = null;

        await _dataPersister.Save();

        treeItemToMove.IsSelected = true;
    }

    private async Task MoveManuscriptElementIntoSection(ManuscriptElementTreeItem treeItemToMove, ManuscriptElementTreeItem destinationTreeItem) {
        Novel.ManuscriptElements.MoveManuscriptElementToList(treeItemToMove.ManuscriptElement, destinationTreeItem.ManuscriptElement.ManuscriptElements);

        var treeItemParentList = treeItemToMove.Parent?.ManuscriptElements ?? ViewModel.Manuscript.ManuscriptElements;
        treeItemParentList.Remove(treeItemToMove);

        destinationTreeItem.ManuscriptElements.Add(treeItemToMove);
        treeItemToMove.Parent = destinationTreeItem;

        await _dataPersister.Save();

        treeItemToMove.IsSelected = true;
        destinationTreeItem.IsExpanded = true;
    }

    private async void ManuscriptSelected() {
        var novelDetailsController = _serviceProvider.CreateInstance<NovelDetailsController>();
        await novelDetailsController.Initialize(Novel);
        ViewModel.EditDataView = novelDetailsController.View;
        ViewModel.ContentView = null;
    }

    private void EventBoardSelected() {
        var eventBoardController = _serviceProvider.CreateInstance<EventBoardController>();
        eventBoardController.Initialize(ShowEditDataView, ShowScene);
        ViewModel.EditDataView = null;
        ViewModel.ContentView = eventBoardController.View;
    }

    private void ShowEditDataView(object? view) {
        ViewModel.EditDataView = view;
    }

    private void ShowScene(Guid id) {
        SelectManuscriptElementId(ViewModel.Manuscript.ManuscriptElements, id);
    }

    private static bool SelectManuscriptElementId(IEnumerable<ManuscriptElementTreeItem> treeItems, Guid id) {
        foreach (var item in treeItems) {
            if (item.ManuscriptElement.Id == id) {
                item.IsSelected = true;
                return true;
            }

            if (SelectManuscriptElementId(item.ManuscriptElements, id)) {
                return true;
            }
        }

        return false;
    }


    private async void ManuscriptElementSelected(ManuscriptElementTreeItem treeItem) {
        if (treeItem.ManuscriptElement.Type == ManuscriptElementType.Section) {
            var novelDetailsController = _serviceProvider.CreateInstance<SectionDetailsController>();
            novelDetailsController.Initialize(treeItem);
            ViewModel.EditDataView = novelDetailsController.View;
            ViewModel.ContentView = null;
            return;
        }

        var sceneDetailsController = _serviceProvider.CreateInstance<SceneDetailsController>();
        await sceneDetailsController.Initialize(treeItem);
        ViewModel.EditDataView = sceneDetailsController.View;
        ViewModel.ContentView = _googleDocController.View;
    }

    private void CharactersSelected() {
        ViewModel.ContentView = null;
        ViewModel.EditDataView = null;
    }

    private async void CharacterSelected(CharacterTreeItem treeItem) {
        var characterDetailsController = _serviceProvider.CreateInstance<CharacterDetailsController>();
        await characterDetailsController.Initialize(treeItem);
        ViewModel.EditDataView = characterDetailsController.View;
        ViewModel.ContentView = _googleDocController.View;
    }

    private async void SupportDocumentSelected(SupportDocumentTreeItem treeItem) {
        var supportDocumentDetailsController = _serviceProvider.CreateInstance<SupportDocumentDetailsController>();
        await supportDocumentDetailsController.Initialize(treeItem);
        ViewModel.EditDataView = supportDocumentDetailsController.View;
        ViewModel.ContentView = _googleDocController.View;
    }


    private async Task AddManuscriptElement(ManuscriptElementTreeItem? parent, ManuscriptElement newManuscriptElement) {
        var newTreeItem = new ManuscriptElementTreeItem(newManuscriptElement, ViewModel, ManuscriptElementSelected) {
            Parent = parent
        };

        if (parent == null) {
            Novel.ManuscriptElements.Add(newManuscriptElement);
            ViewModel.Manuscript.ManuscriptElements.Add(newTreeItem);
        } else {
            parent.ManuscriptElement.ManuscriptElements.Add(newManuscriptElement);
            parent.ManuscriptElements.Add(newTreeItem);
        }

        await _dataPersister.Save();

        newTreeItem.IsSelected = true;
    }

    internal string GetSystemDefaultBrowser() {
        var regKey = Registry.ClassesRoot.OpenSubKey("HTTP\\shell\\open\\command", false);
        if (regKey == null) {
            return string.Empty;
        }

        try {
            //get rid of the enclosing quotes
            var nameObject = regKey.GetValue(null);
            if (nameObject == null) {
                return string.Empty;
            }

            var name = nameObject.ToString()?.ToLower().Replace("" + (char)34, "");
            if (name == null) {
                return string.Empty;
            }

            //check to see if the value ends with .exe (this way we can remove any command line arguments)
            if (!name.EndsWith("exe")) {
                //get rid of all command line arguments (anything after the .exe must go)
                name = name[..(name.LastIndexOf(".exe", StringComparison.Ordinal) + 4)];
            }

            return name;
        }
        catch (Exception ex) {
            MessageBox.Show($"ERROR: An exception of type: {ex.GetType()} occurred in method: {ex.TargetSite} in the following module: {this.GetType()}");
            return string.Empty;
        } finally {
            regKey.Close();
        }
    }

    private void CloseAfterFinishedSaving() {
        _dataPersister.OnFinishedSaving -= CloseAfterFinishedSaving;
        View.Dispatcher.Invoke(() => {
            _dataPersister.CloseNovel();
            _novelClosed.Invoke();
        });
    }

    [Command]
    public async Task CompileNovel() {
        if (MessageBox.Show($"Compile Novel {Novel.Name}?", "Confirmation", MessageBoxButton.YesNo) != MessageBoxResult.Yes) {
            return;
        }

        await _googleDocManager.Compile();

        var address = $"https://docs.google.com/document/d/{Novel.ManuscriptId}";
        Process.Start(GetSystemDefaultBrowser(), address);
    }

    [Command]
    public async Task TypesetNovel() {
        var controller = _serviceProvider.CreateInstance<TypesettingOptionsController>();
        if (controller.View.ShowDialog() == true) {
            await _msWordManager.Compile();
        }
    }

    [Command]
    public void CloseNovel() {
        if (_dataPersister.IsSaving) {
            _dataPersister.OnFinishedSaving += CloseAfterFinishedSaving;
            return;
        }
        _dataPersister.CloseNovel();
        _novelClosed.Invoke();
    }

    [Command]
    public async Task AddSection(ManuscriptElementTreeItem? parent) {
        var section = new ManuscriptElement {
            Name = "New Section",
            Type = ManuscriptElementType.Section,
            IsChapter = true
        };

        await AddManuscriptElement(parent, section);
    }

    [Command]
    public async Task AddScene(ManuscriptElementTreeItem? parent) {
        var scene = new ManuscriptElement {
            Name = "New Scene",
            Type = ManuscriptElementType.Scene
        };

        await AddManuscriptElement(parent, scene);
    }

    [Command]
    public async Task DeleteManuscriptElement(ManuscriptElementTreeItem itemToDelete) {
        if (itemToDelete.Parent == null) {
            Novel.ManuscriptElements.Remove(itemToDelete.ManuscriptElement);
            ViewModel.Manuscript.ManuscriptElements.Remove(itemToDelete);
        } else {
            var parent = itemToDelete.Parent;
            parent.ManuscriptElement.ManuscriptElements.Remove(itemToDelete.ManuscriptElement);
            parent.ManuscriptElements.Remove(itemToDelete);
        }

        await _dataPersister.Save();
    }

    [Command]
    public async Task AddCharacter() {
        var character = new Character {
            Name = "New Character",
        };
        Novel.Characters.Add(character);
        await _dataPersister.Save();

        var treeItem = new CharacterTreeItem(character, CharacterSelected);
        ViewModel.Characters.Characters.Add(treeItem);

        treeItem.IsSelected = true;
    }

    [Command]
    public async Task AddSupportDocument() {
        var supportDocument = new SupportDocument {
            Name = "New Support Document"
        };
        Novel.SupportDocuments.Add(supportDocument);
        await _dataPersister.Save();

        var treeItem = new SupportDocumentTreeItem(supportDocument, SupportDocumentSelected);
        ViewModel.SupportDocuments.Documents.Add(treeItem);
    }

}