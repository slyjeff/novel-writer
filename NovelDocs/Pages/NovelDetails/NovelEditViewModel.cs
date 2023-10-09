using NovelDocs.Entity;
using NovelDocs.PageControls;

namespace NovelDocs.Pages.NovelDetails {
    public abstract class NovelDetailsViewModel : ViewModel {
        private Novel _novel = new();

        internal void SetNovel(Novel novel) {
            _novel = novel;
        }

        public virtual string Name {
            get => _novel.Name;
            set => _novel.Name = value;
        }

        public virtual string Author {
            get => _novel.Author;
            set => _novel.Author = value;
        }

        public virtual string CopyrightYear {
            get => _novel.CopyrightYear;
            set => _novel.CopyrightYear = value;
        }

        public virtual string GoogleDriveFolder {
            get => _novel.GoogleDriveFolder;
            set => _novel.GoogleDriveFolder = value;
        }
    }
}