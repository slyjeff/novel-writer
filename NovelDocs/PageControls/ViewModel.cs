using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace NovelDocs.PageControls; 

public abstract class ViewModel : INotifyPropertyChanged {
    public event PropertyChangedEventHandler? PropertyChanged;

    public virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null) {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}