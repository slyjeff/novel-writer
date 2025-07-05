using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media.Imaging;
using NovelWriter.Entity;
using NovelWriter.Extensions;
using NovelWriter.Managers;
using NovelWriter.PageControls;
using NovelWriter.Pages.CharacterDetails;
using NovelWriter.Pages.EventBoard;
using NovelWriter.Pages.NovelDetails;
using NovelWriter.Pages.RichTextEditor;
using NovelWriter.Pages.SceneDetails;
using NovelWriter.Pages.SectionDetails;
using NovelWriter.Pages.SupportDocumentDetails;
using NovelWriter.Pages.TypesettingOptions;
using NovelWriter.Services;
using Task = System.Threading.Tasks.Task;
// ReSharper disable AsyncVoidMethod

namespace NovelWriter.Pages.NovelEdit; 

// ReSharper disable ClassNeverInstantiated.Global
internal sealed class NovelEditController : Controller<NovelEditView, NovelEditViewModel> {
    private readonly IServiceProvider _serviceProvider;
    private readonly IDataPersister _dataPersister;
    private readonly IRichTextEditorController _richTextEditorController;
    private readonly IMsWordManager _msWordManager;
    private Action _novelClosed = null!; //will never be null because initialize will always be called

    public NovelEditController(IServiceProvider serviceProvider, IDataPersister dataPersister, IRichTextEditorController richTextEditorController, IMsWordManager msWordManager) {
        _serviceProvider = serviceProvider;
        _dataPersister = dataPersister;
        _richTextEditorController = richTextEditorController;
        _msWordManager = msWordManager;

        View.OnMoveNovelTreeItem += MoveNovelTreeItem;
        ViewModel.NavigatorWidth = new GridLength(_dataPersister.AppData.NavigatorWidth);
        
        ViewModel.PropertyChanged += async (_, e) => {
            if (e.PropertyName == nameof(ViewModel.NavigatorWidth)) {
                _dataPersister.AppData.NavigatorWidth = (int)Math.Round(ViewModel.NavigatorWidth.Value);
                await dataPersister.Save();    
            }
        };
    }

    private Novel Novel => _dataPersister.CurrentNovel;

    public async Task Initialize(Action novelClosed) {
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
            var image = await _dataPersister.GetImage(character, 50);
            var treeItem = new CharacterTreeItem(character, image, ViewModel, CharacterSelected);
            ViewModel.Characters.Characters.Add(treeItem);
        }

        foreach (var supportDocument in Novel.SupportDocuments) {
            var treeItem = new SupportDocumentTreeItem(supportDocument, ViewModel, SupportDocumentSelected);
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

    private async void EventBoardSelected() {
        var eventBoardController = _serviceProvider.CreateInstance<EventBoardController>();
        await eventBoardController.Initialize(ShowEditDataView, ShowScene);
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
        ViewModel.ContentView = _richTextEditorController.View;
    }

    private void CharactersSelected() {
        ViewModel.ContentView = null;
        ViewModel.EditDataView = null;
    }

    private async void CharacterSelected(CharacterTreeItem treeItem) {
        var characterDetailsController = _serviceProvider.CreateInstance<CharacterDetailsController>();
        await characterDetailsController.Initialize(treeItem);
        ViewModel.EditDataView = characterDetailsController.View;
        ViewModel.ContentView = _richTextEditorController.View;
    }

    private async void SupportDocumentSelected(SupportDocumentTreeItem treeItem) {
        var supportDocumentDetailsController = _serviceProvider.CreateInstance<SupportDocumentDetailsController>();
        await supportDocumentDetailsController.Initialize(treeItem);
        ViewModel.EditDataView = supportDocumentDetailsController.View;
        ViewModel.ContentView = _richTextEditorController.View;
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

    [Command]
    public Task CompileNovel() {
        if (MessageBox.Show($"Compile Novel {Novel.Name}?", "Confirmation", MessageBoxButton.YesNo) != MessageBoxResult.Yes) {
            return Task.CompletedTask;
        }
        return Task.CompletedTask;
        /*await _googleDocManager.Compile();

        var address = $"https://docs.google.com/document/d/{Novel.ManuscriptId}";
        Process.Start(GetSystemDefaultBrowser(), address);*/
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
        if (MessageBox.Show($"Delete {itemToDelete.Name}?", "Confirm Delete", MessageBoxButton.YesNo) != MessageBoxResult.Yes) {
            return;
        }

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
        
        var defaultImage = new BitmapImage (
            new Uri(new Random().Next(0, 2) == 1 ? "/images/Character.png" : "/images/Character2.png", UriKind.Relative)
        );
        await _dataPersister.SaveImage(character, defaultImage);

        var treeItem = new CharacterTreeItem(character, defaultImage, ViewModel, CharacterSelected);
        ViewModel.Characters.Characters.Add(treeItem);

        treeItem.IsSelected = true;
    }

    [Command]
    public async Task DeleteCharacter(CharacterTreeItem characterToDelete) {
        if (MessageBox.Show($"Delete {characterToDelete.Name}?", "Confirm Delete", MessageBoxButton.YesNo) != MessageBoxResult.Yes) {
            return;
        }

        Novel.Characters.Remove(characterToDelete.Character);
        var eventBoardCharacterToRemove = Novel.EventBoardCharacters.FirstOrDefault(x => x.Id == characterToDelete.Character.Id);
        if (eventBoardCharacterToRemove != null) {
            Novel.EventBoardCharacters.Remove(eventBoardCharacterToRemove);
        }
        await _dataPersister.Save();

        ViewModel.Characters.Characters.Remove(characterToDelete);
    }

    [Command]
    public async Task AddSupportDocument() {
        var supportDocument = new SupportDocument {
            Name = "New Support Document"
        };
        Novel.SupportDocuments.Add(supportDocument);
        await _dataPersister.Save();

        var treeItem = new SupportDocumentTreeItem(supportDocument, ViewModel,SupportDocumentSelected);
        ViewModel.SupportDocuments.Documents.Add(treeItem);
    }

    [Command]
    public async Task DeleteSupportDocument(SupportDocumentTreeItem supportDocumentToDelete) {
        if (MessageBox.Show($"Delete {supportDocumentToDelete.Name}?", "Confirm Delete", MessageBoxButton.YesNo) != MessageBoxResult.Yes) {
            return;
        }

        Novel.SupportDocuments.Remove(supportDocumentToDelete.SupportDocument);
        await _dataPersister.Save();

        ViewModel.SupportDocuments.Documents.Remove(supportDocumentToDelete);
    }
}