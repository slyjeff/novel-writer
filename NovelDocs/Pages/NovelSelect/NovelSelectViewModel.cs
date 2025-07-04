using System.Collections.Generic;
using NovelDocs.Entity;
using NovelDocs.PageControls;

namespace NovelDocs.Pages.NovelSelect {
    public abstract class NovelSelectViewModel : ViewModel {
        public virtual IList<NovelSelectAction> Novels { get; set; } = new List<NovelSelectAction>();
    }

    public sealed class NovelSelectAction {
        public NovelSelectAction(NovelData? novel) {
            NovelData = novel;
            if (novel == null) {
                Action = "New Novel";
                ImageUriSource = "/images/EmptyDocument.png";
                LastModified = string.Empty;
            } else {
                Action = novel.Name;
                ImageUriSource = "/images/Document.png";
                LastModified = novel.LastModified.ToString("MM/dd/yyyy hh:mm tt");
            }
        }

        public NovelData? NovelData { get; }

        public string Action { get; }
        public string ImageUriSource { get; }
        public string LastModified { get; }
    }
}