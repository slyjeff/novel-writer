using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using NovelDocs.Extensions;
using NovelDocs.Initialization;
using NovelDocs.Managers;
using NovelDocs.PageControls;
using NovelDocs.Pages.CompileStatus;
using NovelDocs.Pages.GoogleDoc;
using NovelDocs.Pages.Main;
using NovelDocs.Services;

namespace NovelDocs; 

public partial class App {
    private readonly ServiceProvider _serviceProvider;

    public App() {
        var services = new ServiceCollection();
        AddServices(services);
        _serviceProvider = services.BuildServiceProvider();

        PageControllerConfiguration.PageDependencyResolver = new PageDependencyResolver(_serviceProvider);
    }

    private static void AddServices(IServiceCollection services) {
        services
            .AddSingleton<IDataPersister, DataPersister>()
            .AddSingleton<IGoogleDocController, GoogleDocController>()
            .AddTransient<IGoogleDocManager, GoogleDocManager>()
            .AddTransient<IGoogleDocService, GoogleDocService>()
            .AddTransient<IMsWordManager, MsWordManager>()
            .AddTransient<ICompileStatusController, CompileStatusController>()
            .AddTransient<ICompileStatusService, CompileStatusService>();
    }

    private void App_OnStartup(object sender, StartupEventArgs e) {
        var mainController = _serviceProvider.CreateInstance<MainController>();
        mainController.View.Show();
    }
}