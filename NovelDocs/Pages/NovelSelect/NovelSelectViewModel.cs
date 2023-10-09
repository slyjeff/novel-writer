using System.Collections.Generic;
using NovelDocs.Entity;
using NovelDocs.PageControls;

namespace NovelDocs.Pages.NovelSelect {
    public abstract class NovelSelectViewModel : ViewModel {
        public virtual IList<NovelSelectAction> Novels { get; set; } = new List<NovelSelectAction>();
    }

    public sealed class NovelSelectAction {
        public NovelSelectAction(Novel? novel) {
            Novel = novel;
            if (novel == null) {
                Action = "New Novel";
                UriSource = "/images/EmptyDocument.png";
                LastModified = string.Empty;
            } else {
                Action = novel.Name;
                UriSource = "/images/Document.png";
                LastModified = novel.LastModified.ToString("MM/dd/yyyy hh:mm tt");
            }
        }

        public Novel? Novel { get; }

        public string Action { get; }
        public string UriSource { get; }
        public string LastModified { get; }
    }
}