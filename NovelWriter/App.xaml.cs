using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using NovelWriter.Extensions;
using NovelWriter.Initialization;
using NovelWriter.Managers;
using NovelWriter.PageControls;
using NovelWriter.Pages.CompileStatus;
using NovelWriter.Pages.Main;
using NovelWriter.Pages.RichTextEditor;
using NovelWriter.Services;

namespace NovelWriter; 

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
            .AddSingleton<IRichTextEditorController, RichTextEditorController>()
            .AddTransient<IGoogleDocService, GoogleDocService>()
            .AddTransient<IMsWordManager, MsWordManager>()
            .AddTransient<IThrottledSaver, ThrottledSaver>()
            .AddTransient<ICompileStatusController, CompileStatusController>()
            .AddTransient<ICompileStatusService, CompileStatusService>();
    }

    private async void App_OnStartup(object sender, StartupEventArgs e) {
        var mainController = _serviceProvider.CreateInstance<MainController>();
        await mainController.Initialize();
        mainController.View.Show();
    }

    private async void App_Exit(object sender, ExitEventArgs e) {
        await _serviceProvider.DisposeAsync();
    }
}