using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using NovelDocs.Extensions;
using NovelDocs.Initialization;
using NovelDocs.PageControls;
using NovelDocs.Pages.Main;
using NovelDocs.Services;

namespace NovelDocs; 

public partial class App {
    private readonly ServiceProvider _serviceProvider;

    public App() {
        var services = new ServiceCollection();
        services.AddSingleton<IDataPersister, DataPersister>();
        _serviceProvider = services.BuildServiceProvider();

        PageControllerConfiguration.PageDependencyResolver = new PageDependencyResolver(_serviceProvider);
    }

    private void App_OnStartup(object sender, StartupEventArgs e) {
        var mainController = _serviceProvider.CreateInstance<MainController>();
        mainController.View.Show();
    }
}