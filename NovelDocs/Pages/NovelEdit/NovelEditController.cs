using System;
using NovelDocs.Entity;
using NovelDocs.Extensions;
using NovelDocs.PageControls;
using NovelDocs.Pages.CharacterDetails;
using NovelDocs.Pages.GoogleDoc;
using NovelDocs.Pages.NovelDetails;
using NovelDocs.Pages.SceneDetails;
using NovelDocs.Pages.SectionDetails;
using NovelDocs.Services;

namespace NovelDocs.Pages.NovelEdit; 

internal sealed class NovelEditController : Controller<NovelEditView, NovelEditViewModel> {
    private readonly IServiceProvider _serviceProvider;
    private readonly IDataPersister _dataPersister;
    private Action _novelClosed = null!; //will never be null because initialize will always be called
    private Novel _novel = null!; //will never be null because initialize will always be called

    public NovelEditController(IServiceProvider serviceProvider, IDataPersister dataPersister, IGoogleDocController googleDocController) {
        _serviceProvider = serviceProvider;
        _dataPersister = dataPersister;

        ViewModel.GoogleDocView = googleDocController.View;

        View.OnMoveNovelTreeItem += MoveNovelTreeItem;
    }

    public void Initialize(Action novelClosed, Novel novelToLoad) {
        _novelClosed = novelClosed;
        _novel = novelToLoad;

        _dataPersister.Data.LastOpenedNovel = novelToLoad.Name;
        _dataPersister.Save();

        ViewModel.Manuscript.Selected += ManuscriptSelected;
        ViewModel.Characters.Selected += CharactersSelected;

        foreach (var element in _novel.ManuscriptElements) {
            if (element.Type != ManuscriptElementType.Section) {
                continue;
            }

            var treeItem = new ManuscriptElementTreeItem(element, ViewModel, ManuscriptElementSelected);
            ViewModel.Manuscript.ManuscriptElements.Add(treeItem);
        }

        ViewModel.Manuscript.IsSelected = true;

        foreach (var character in _novel.Characters) {
            var treeItem = new CharacterTreeItem(character, CharacterSelected);
            ViewModel.Characters.Characters.Add(treeItem);
        }
    }

    private void MoveNovelTreeItem(NovelTreeItem itemToMove, MoveDestination moveDestination, NovelTreeItem destinationItem) {
        if (itemToMove is CharacterTreeItem characterToMove) {
            if (destinationItem is CharacterTreeItem characterDestination) {
                _novel.Characters.Move(characterToMove.Character, characterDestination.Character);
                ViewModel.Characters.Characters.Move(characterToMove, characterDestination);

                _dataPersister.Save();
                
                characterToMove.IsSelected = true;
            }
        }

        if (itemToMove is ManuscriptElementTreeItem manuscriptElementToMoveTreeItem) {
            if (destinationItem is ManuscriptTreeItem) {
                _novel.ManuscriptElements.MoveManuscriptElementToList(manuscriptElementToMoveTreeItem.ManuscriptElement, _novel.ManuscriptElements);
                if (manuscriptElementToMoveTreeItem.Parent != null) {
                    manuscriptElementToMoveTreeItem.Parent.ManuscriptElements.Remove(manuscriptElementToMoveTreeItem);
                    ViewModel.Manuscript.ManuscriptElements.Add(manuscriptElementToMoveTreeItem);
                    manuscriptElementToMoveTreeItem.Parent = null;
                }

                _dataPersister.Save();

                manuscriptElementToMoveTreeItem.IsSelected = true;
                if (manuscriptElementToMoveTreeItem.Parent != null) {
                    manuscriptElementToMoveTreeItem.Parent.IsExpanded = true;
                }
            }

            if (destinationItem is ManuscriptElementTreeItem destinationManuscriptElementTreeItem) {
                if (moveDestination == MoveDestination.Into) {
                    _novel.ManuscriptElements.MoveManuscriptElementToList(manuscriptElementToMoveTreeItem.ManuscriptElement, destinationManuscriptElementTreeItem.ManuscriptElement.ManuscriptElements);

                    if (manuscriptElementToMoveTreeItem.Parent != null) {
                        manuscriptElementToMoveTreeItem.Parent.ManuscriptElements.Remove(manuscriptElementToMoveTreeItem);
                    } else {
                        ViewModel.Manuscript.ManuscriptElements.Remove(manuscriptElementToMoveTreeItem);
                    }

                    destinationManuscriptElementTreeItem.ManuscriptElements.Add(manuscriptElementToMoveTreeItem);
                    manuscriptElementToMoveTreeItem.Parent = destinationManuscriptElementTreeItem;
                } else {
                    var destinationParentList = _novel.ManuscriptElements.FindParentManuscriptElementList(destinationManuscriptElementTreeItem.ManuscriptElement);
                    if (destinationParentList == null) {
                        return;
                    }
                    _novel.ManuscriptElements.MoveManuscriptElementToList(manuscriptElementToMoveTreeItem.ManuscriptElement, destinationParentList);
                    destinationParentList.Move(manuscriptElementToMoveTreeItem.ManuscriptElement, destinationManuscriptElementTreeItem.ManuscriptElement);

                    var treeItemDestinationParentList = destinationManuscriptElementTreeItem.Parent?.ManuscriptElements ?? ViewModel.Manuscript.ManuscriptElements;
                    treeItemDestinationParentList.MoveManuscriptElementTreeItemToList(manuscriptElementToMoveTreeItem);
                    treeItemDestinationParentList.Move(manuscriptElementToMoveTreeItem, destinationManuscriptElementTreeItem);
                    manuscriptElementToMoveTreeItem.Parent = destinationManuscriptElementTreeItem.Parent;
                }

                _dataPersister.Save();

                manuscriptElementToMoveTreeItem.IsSelected = true;
                if (manuscriptElementToMoveTreeItem.Parent != null) {
                    manuscriptElementToMoveTreeItem.Parent.IsExpanded = true;
                }
            }
        }
    }


    private void ManuscriptSelected() {
        var novelDetailsController = _serviceProvider.CreateInstance<NovelDetailsController>();
        novelDetailsController.Initialize(_novel);
        ViewModel.EditDataView = novelDetailsController.View;
    }

    private async void ManuscriptElementSelected(ManuscriptElementTreeItem treeItem) {
        if (treeItem.ManuscriptElement.Type == ManuscriptElementType.Section) {
            var novelDetailsController = _serviceProvider.CreateInstance<SectionDetailsController>();
            novelDetailsController.Initialize(treeItem);
            ViewModel.EditDataView = novelDetailsController.View;
            return;
        }

        var sceneDetailsController = _serviceProvider.CreateInstance<SceneDetailsController>();
        await sceneDetailsController.Initialize(treeItem);
        ViewModel.EditDataView = sceneDetailsController.View;
    }

    private void CharactersSelected() {
        ViewModel.EditDataView = null!;
    }

    private async void CharacterSelected(CharacterTreeItem treeItem) {
        var characterDetailsController = _serviceProvider.CreateInstance<CharacterDetailsController>();
        await characterDetailsController.Initialize(treeItem);
        ViewModel.EditDataView = characterDetailsController.View;
    }

    private void AddManuscriptElement(ManuscriptElementTreeItem? parent, ManuscriptElement newManuscriptElement) {
        var newTreeItem = new ManuscriptElementTreeItem(newManuscriptElement, ViewModel, ManuscriptElementSelected) {
            Parent = parent
        };
        if (parent == null) {
            _novel.ManuscriptElements.Add(newManuscriptElement);
            ViewModel.Manuscript.ManuscriptElements.Add(newTreeItem);
        } else {
            parent.ManuscriptElement.ManuscriptElements.Add(newManuscriptElement);
            parent.ManuscriptElements.Add(newTreeItem);
        }

        _dataPersister.Save();

        newTreeItem.IsSelected = true;
    }

    [Command]
    public void CloseNovel() {
        _dataPersister.Data.LastOpenedNovel = string.Empty;
        _dataPersister.Save();
        _novelClosed.Invoke();
    }

    [Command]
    public void AddSection(ManuscriptElementTreeItem? parent) {
        var section = new ManuscriptElement {
            Name = "New Section",
            Type = ManuscriptElementType.Section
        };

        AddManuscriptElement(parent, section);
    }

    [Command]
    public void AddScene(ManuscriptElementTreeItem? parent) {
        var scene = new ManuscriptElement {
            Name = "New Scene",
            Type = ManuscriptElementType.Scene
        };

        AddManuscriptElement(parent, scene);
    }

    [Command]
    public void DeleteManuscriptElement(ManuscriptElementTreeItem itemToDelete) {
        if (itemToDelete.Parent == null) {
            _novel.ManuscriptElements.Remove(itemToDelete.ManuscriptElement);
            ViewModel.Manuscript.ManuscriptElements.Remove(itemToDelete);
        } else {
            var parent = itemToDelete.Parent;
            parent.ManuscriptElement.ManuscriptElements.Remove(itemToDelete.ManuscriptElement);
            parent.ManuscriptElements.Remove(itemToDelete);
        }

        _dataPersister.Save();
    }

    [Command]
    public void AddCharacter() {
        var character = new Character {
            Name = "New Character",
        };
        _novel.Characters.Add(character);
        _dataPersister.Save();

        var treeItem = new CharacterTreeItem(character, CharacterSelected);
        ViewModel.Characters.Characters.Add(treeItem);

        treeItem.IsSelected = true;
    }
}