using System;
using NovelDocs.Entity;
using NovelDocs.Extensions;
using NovelDocs.PageControls;
using NovelDocs.Pages.CharacterDetails;
using NovelDocs.Pages.GoogleDoc;
using NovelDocs.Pages.NovelDetails;
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

            var treeItem = new ManuscriptElementTreeItem(element, SectionSelected);
            ViewModel.Manuscript.ManuscriptElements.Add(treeItem);
        }

        ViewModel.Manuscript.IsSelected = true;

        foreach (var character in _novel.Characters) {
            var treeItem = new CharacterTreeItem(character, CharacterSelected);
            ViewModel.Characters.Characters.Add(treeItem);
        }
    }

    private void ManuscriptSelected() {
        var novelDetailsController = _serviceProvider.CreateInstance<NovelDetailsController>();
        novelDetailsController.Initialize(_novel);
        ViewModel.EditDataView = novelDetailsController.View;
    }

    private void SectionSelected(ManuscriptElementTreeItem treeItem) {
        var novelDetailsController = _serviceProvider.CreateInstance<SectionDetailsController>();
        novelDetailsController.Initialize(treeItem);
        ViewModel.EditDataView = novelDetailsController.View;
    }

    private void CharactersSelected() {
        ViewModel.EditDataView = null!;
    }

    private void CharacterSelected(CharacterTreeItem treeItem) {
        var characterDetailsController = _serviceProvider.CreateInstance<CharacterDetailsController>();
        characterDetailsController.Initialize(treeItem);
        ViewModel.EditDataView = characterDetailsController.View;
    }

    [Command]
    public void CloseNovel() {
        _dataPersister.Data.LastOpenedNovel = string.Empty;
        _dataPersister.Save();
        _novelClosed.Invoke();
    }

    [Command]
    public void AddSection() {
        var section = new ManuscriptElement {
            Name = "New Section",
            Type = ManuscriptElementType.Section
        };
        _novel.ManuscriptElements.Add(section);
        _dataPersister.Save();

        var treeItem = new ManuscriptElementTreeItem(section, SectionSelected);
        ViewModel.Manuscript.ManuscriptElements.Add(treeItem);
        treeItem.IsSelected = true;
    }

    [Command]
    public void AddChapter() {
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