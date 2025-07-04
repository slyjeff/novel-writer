using System.Collections.Generic;
using NovelWriter.Entity;
using NovelWriter.PageControls;

namespace NovelWriter.Pages.TypesettingOptions; 

public class TypesettingOptionsViewModel : ViewModel {
    public virtual IList<string> Fonts { get; set; } = new List<string>();
    public virtual IList<int> FontSizes { get; set; } = new List<int>();

    private Novel _novel = null!; // set in the constructor of the controller

    public virtual Novel Novel {
        get => _novel;
        set {
            _novel = value;
            OnPropertyChanged(nameof(TitleFont));
            OnPropertyChanged(nameof(HeaderFont));
            OnPropertyChanged(nameof(HeaderFontSize));
            OnPropertyChanged(nameof(PageNumberFont));
            OnPropertyChanged(nameof(PageNumberFontSize));
            OnPropertyChanged(nameof(ChapterFont));
            OnPropertyChanged(nameof(BodyFont));
            OnPropertyChanged(nameof(BodyFontSize));
        }
    }

    public virtual string TitleFont {
        get => _novel.Typesetting.TitleFont;
        set => _novel.Typesetting.TitleFont = value;
    }

    public virtual string HeaderFont {
        get => _novel.Typesetting.HeaderFont;
        set => _novel.Typesetting.HeaderFont = value;
    }
    public virtual int HeaderFontSize {
        get => _novel.Typesetting.HeaderFontSize;
        set => _novel.Typesetting.HeaderFontSize = value;
    }
    public virtual string PageNumberFont {
        get => _novel.Typesetting.PageNumberFont;
        set => _novel.Typesetting.PageNumberFont = value;
    }
    public virtual int PageNumberFontSize {
        get => _novel.Typesetting.PageNumberFontSize;
        set => _novel.Typesetting.PageNumberFontSize = value;
    }
    public virtual string ChapterFont {
        get => _novel.Typesetting.ChapterFont;
        set => _novel.Typesetting.ChapterFont = value;
    }
    public virtual string BodyFont {
        get => _novel.Typesetting.BodyFont;
        set => _novel.Typesetting.BodyFont = value;
    }
    public virtual int BodyFontSize {
        get => _novel.Typesetting.BodyFontSize;
        set => _novel.Typesetting.BodyFontSize = value;
    }
}