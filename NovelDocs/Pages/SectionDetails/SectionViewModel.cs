using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using NovelDocs.Entity;
using NovelDocs.PageControls;

namespace NovelDocs.Pages.SectionDetails; 

public abstract class SectionDetailsViewModel : ViewModel {
    private ManuscriptElement _section = new();

    internal void SetSection(ManuscriptElement section) {
        _section = section;

        foreach (var plotPoint in _section.PlotPoints) {
            var plotPointViewModel = new PlotPointViewModel(plotPoint);
            plotPointViewModel.PropertyChanged += (_, _) => {
                OnPropertyChanged(nameof(PlotPoints));
            };
            PlotPoints.Add(plotPointViewModel);
        }
    }

    public virtual string Name {
        get => _section.Name;
        set => _section.Name = value;
    }

    public virtual bool IsChapter {
        get => _section.IsChapter;
        set => _section.IsChapter = value;
    }

    public virtual string Summary {
        get => _section.Summary;
        set => _section.Summary = value;
    }

    public ObservableCollection<PlotPointViewModel> PlotPoints { get; } = new();
}

public sealed class PlotPointViewModel : INotifyPropertyChanged {
    private readonly PlotPoint _plotPoint;

    public PlotPointViewModel(PlotPoint plotPoint) {
        _plotPoint = plotPoint;
    }

    public bool Completed {
        get => _plotPoint.Completed;
        set {
            if (value == _plotPoint.Completed) {
                return;
            }

            _plotPoint.Completed = value;
            OnPropertyChanged();
        }
    }

    public string Details {
        get => _plotPoint.Details;
        set {
            if (value == _plotPoint.Details) {
                return;
            }

            _plotPoint.Details = value;
            OnPropertyChanged();
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null) {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}