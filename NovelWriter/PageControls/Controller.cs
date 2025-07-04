using System;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace NovelWriter.PageControls; 

public interface IController {
    object View { get; }
    ViewModel ViewModel { get; }
}

public abstract class Controller : IController {
    public object View { get; protected set; } = null!;
    public ViewModel ViewModel { get; protected set; } = null!;
}

public abstract class Controller<TView, TViewModel> : Controller 
    where TView : class 
    where TViewModel : ViewModel {
        
    #region Create Controller/View/ViewModel

    protected Controller() {
        var view = PageControllerConfiguration.PageDependencyResolver.Resolve<TView>();
        View = view ?? throw new Exception($"Could not create View for type {typeof(TView).Name}");
        ViewModel = CreateViewModel();

        var dataContextProperty = View.GetType().GetProperty("DataContext");
        if (dataContextProperty == null) {
            return;
        }
        dataContextProperty.SetValue(View, ViewModel, Array.Empty<object>());
    }

    private async void PropertyChanged(object? sender, PropertyChangedEventArgs? e) {
        var query = from method in GetType().GetMethods()
            where method.GetCustomAttributes(typeof(PropertyChangedAttribute), true).Any()
            where e?.PropertyName + "Changed" == method.Name
            select method;

        var propertyChangedMethod = query.FirstOrDefault();
        if (propertyChangedMethod == null) {
            return;
        }

        if (propertyChangedMethod.Invoke(this, Array.Empty<object>()) is Task task) {
            await task;
        }
    }

    private TViewModel CreateViewModel() {
        var viewModelProxyType = DynamicViewModelManager.AddCommandsToViewModelClass(typeof(TViewModel), GetType());

        var constructor = viewModelProxyType.GetConstructors()[0];

        var parameters = constructor.GetParameters().Select(parameter => PageControllerConfiguration.PageDependencyResolver.Resolve(parameter.ParameterType)).ToArray();
        if (Activator.CreateInstance(viewModelProxyType, parameters) is not TViewModel viewModel) {
            throw new Exception($"Could not create ViewModel for type {typeof(TViewModel).Name}");
        }

        var query = from method in GetType().GetMethods()
            where method.GetCustomAttributes(typeof(CommandAttribute), true).Any()
            select method;

        foreach (var method in query.ToList()) {
            var property = viewModelProxyType.GetProperty(method.Name);
            var command = method.ReturnType == typeof(Task) ? (ICommand)new CommandAsync(this, method.Name) : new Command(this, method.Name);
            property?.SetValue(viewModel, command, Array.Empty<object>());
        }

        if (viewModel is INotifyPropertyChanged notifyPropertyChanged) {
            notifyPropertyChanged.PropertyChanged += PropertyChanged;
        }

        return viewModel;
    }

    #endregion

    public new TViewModel ViewModel {
        get => (TViewModel)base.ViewModel;
        private init => base.ViewModel = value;
    }

    public new TView View {
        get => (TView)base.View;
        private init => base.View = value;
    }
}