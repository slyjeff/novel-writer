using NovelDocs.Entity;
using NovelDocs.PageControls;

namespace NovelDocs.Pages.SectionDetails {
    public abstract class SectionDetailsViewModel : ViewModel {
        private ManuscriptElement _section = new();

        internal void SetSection(ManuscriptElement section) {
            _section = section;
        }

        public virtual string Name {
            get => _section.Name;
            set => _section.Name = value;
        }
    }
}