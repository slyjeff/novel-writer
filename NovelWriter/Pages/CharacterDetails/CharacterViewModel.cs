using System;
using System.Windows.Media.Imaging;
using NovelWriter.Entity;

namespace NovelWriter.Pages.CharacterDetails;

public abstract class CharacterDetailsViewModel : RichTextViewModel<Character> {
    public virtual BitmapImage Image { get; set; } = new(
        new Uri(new Random().Next(0, 2) == 1 ? "/images/Character.png" : "/images/Character2.png", UriKind.Relative)
    );
}
