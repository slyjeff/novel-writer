using System.ComponentModel;
using System.Threading.Tasks;
using CefSharp;
using CefSharp.Wpf;
using NovelDocs.PageControls;
using NovelDocs.Services;

namespace NovelDocs.Pages.GoogleDoc;

public interface IGoogleDocController {
    GoogleDocView View { get; }
    Task Show(IGoogleDocViewModel googleDocViewModel);
    void Hide();
}

internal sealed class GoogleDocController : Controller<GoogleDocView, GoogleDocViewModel>, IGoogleDocController {
    private readonly IGoogleDocService _googleDocService;
    private readonly ChromiumWebBrowser _browser;
    private IGoogleDocViewModel? _googleDocViewModel;

    public GoogleDocController(IGoogleDocService googleDocService) {
        _googleDocService = googleDocService;
        var settings = new CefSettings {
            UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/96.0.4664.110 Safari/537.36 /CefSharp Browser" + Cef.CefSharpVersion,
            CachePath = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location)
        };
        Cef.Initialize(settings);

        _browser = new ChromiumWebBrowser();

        View.BrowserGrid.Children.Add(_browser);
    }

    public async Task Show(IGoogleDocViewModel googleDocViewModel) {
        if (_googleDocViewModel != null) {
            _googleDocViewModel.PropertyChanged -= GoogleDocViewModelPropertyChanged;
        }

        _googleDocViewModel = googleDocViewModel;
        _googleDocViewModel.PropertyChanged += GoogleDocViewModelPropertyChanged;
        await BrowseToDoc();
        ViewModel.IsVisible = true;
    }

    private async void GoogleDocViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e) {
        if (e.PropertyName == nameof(GoogleDocViewModel.GoogleDocId)) {
            await BrowseToDoc();
        }
    }

    public void Hide() {
        ViewModel.IsVisible = false;

        if (_googleDocViewModel == null) {
            return;
        }

        _googleDocViewModel.PropertyChanged -= GoogleDocViewModelPropertyChanged;
        _googleDocViewModel = null;
    }

    private async Task BrowseToDoc() {
        if (_googleDocViewModel == null) {
            return;
        }

        if (!await _googleDocService.GoogleDocExists(_googleDocViewModel.GoogleDocId)) {
            ViewModel.DocumentExists = false;
            return;
        }

        var address = $"https://docs.google.com/document/d/{_googleDocViewModel.GoogleDocId}/edit";
        if (_browser.Address == null || !_browser.Address.StartsWith(address)) {
            _browser.Address = address;
        }

        ViewModel.DocumentExists = true;
    }

    [Command]
    public async Task CreateDocument() {
        if (_googleDocViewModel == null) {
            return;
        }

        _googleDocViewModel.GoogleDocId = await _googleDocService.CreateDocument(_googleDocViewModel.Name);
    }

    [Command]
    public void AssignDocument() {
        ViewModel.AssigningExistingDocument = true;
    }

    [Command]
    public void AssigningExistingDocumentCanceled() {
        ViewModel.AssigningExistingDocument = false;
    }

    [Command]
    public void AssigningExistingDocumentConfirmed() {
        if (_googleDocViewModel == null) {
            return;
        }

        _googleDocViewModel.GoogleDocId = ViewModel.GoogleDocId;
        ViewModel.AssigningExistingDocument = false;
    }
}