using System.Collections.Generic;
using System.Printing;
using NovelDocs.Entity;
using NovelDocs.Pages.NovelEdit;

namespace NovelDocs.Extensions; 

internal static class ListExtensions {
    public static void Move<T>(this IList<T> list, T itemToMove, T destination) {
        list.Remove(itemToMove);
        list.Insert(list.IndexOf(destination), itemToMove);
    }

    public static IList<ManuscriptElement>? FindParentManuscriptElementList(this IList<ManuscriptElement> list, ManuscriptElement item) {
        if (list.Contains(item)) {
            return list;
        }

        foreach (var listItem in list) {
            var parentList = listItem.ManuscriptElements.FindParentManuscriptElementList(item);
            if (parentList != null) {
                return parentList;
            }
        }

        return null;
    }
    

    public static void MoveManuscriptElementToList(this IList<ManuscriptElement> manuscriptElements, ManuscriptElement itemToMove, IList<ManuscriptElement> destinationList) {
        manuscriptElements.RemoveManuscriptElement(itemToMove);
        destinationList.Add(itemToMove);
    }

    private static bool RemoveManuscriptElement(this ICollection<ManuscriptElement> manuscriptElements, ManuscriptElement itemToRemove) {
        if (manuscriptElements.Remove(itemToRemove)) {
            return true;
        }

        foreach (var manuscriptElement in manuscriptElements) {
            if (manuscriptElement.ManuscriptElements.RemoveManuscriptElement(itemToRemove)) {
                return true;
            }
        }

        return false;
    }

    public static void MoveManuscriptElementTreeItemToList(this IList<ManuscriptElementTreeItem> manuscriptElements, ManuscriptElementTreeItem itemToMove) {
        if (itemToMove.Parent == null) {
            return;
        }

        itemToMove.Parent.ManuscriptElements.Remove(itemToMove);
        manuscriptElements.Add(itemToMove);
    }

    private static bool RemoveManuscriptElementTreeItem(this ICollection<ManuscriptElementTreeItem> manuscriptElements, ManuscriptElementTreeItem itemToRemove) {
        if (manuscriptElements.Remove(itemToRemove)) {
            return true;
        }

        foreach (var manuscriptElement in manuscriptElements) {
            if (manuscriptElement.ManuscriptElements.RemoveManuscriptElementTreeItem(itemToRemove)) {
                return true;
            }
        }

        return false;
    }

}