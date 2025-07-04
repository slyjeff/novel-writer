using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Timers;
using CefSharp;
using CefSharp.Wpf;
using NovelWriter.Managers;
using NovelWriter.PageControls;

namespace NovelWriter.Pages.GoogleDoc;

public interface IGoogleDocController {
    GoogleDocView View { get; }
    Task Show(IGoogleDocViewModel googleDocViewModel);
    void Hide();
}

internal sealed class GoogleDocController : Controller<GoogleDocView, GoogleDocViewModel>, IGoogleDocController {
    private readonly IGoogleDocManager _googleDocManager;
    private IGoogleDocViewModel? _googleDocViewModel;

    private readonly IDictionary<string, ChromiumWebBrowser> _browsers = new Dictionary<string, ChromiumWebBrowser>();

    public GoogleDocController(IGoogleDocManager googleDocService) {
        _googleDocManager = googleDocService;
        var settings = new CefSettings {
            UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/96.0.4664.110 Safari/537.36 /CefSharp Browser" + Cef.CefSharpVersion,
            CachePath = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location)
        };
        Cef.Initialize(settings);
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

    private readonly Timer _renameTimer = new(1000);

    private async void GoogleDocViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e) {
        switch (e.PropertyName) {
            case nameof(IGoogleDocViewModel.GoogleDocId):
                await BrowseToDoc();
                break;
            case nameof(IGoogleDocViewModel.Name): {
                //for renames, wait a second to send all renames at once.
                if (_renameTimer.Enabled) {
                    _renameTimer.Stop();
                } else {
                    _renameTimer.Elapsed += RenameDoc;
                }

                _renameTimer.Start();
                break;
            }
        }
    }

    private async void RenameDoc(object? sender, ElapsedEventArgs e) {
        if (_googleDocViewModel == null) {
            return;
        }

        await _googleDocManager.RenameDoc(_googleDocViewModel.GoogleDocId, _googleDocViewModel.Name);

        _renameTimer.Elapsed -= RenameDoc;
        _renameTimer.Stop();
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

        if (!await _googleDocManager.GoogleDocExists(_googleDocViewModel.GoogleDocId)) {
            ViewModel.DocumentExists = false;
            return;
        }

        var browser = GetBrowser(_googleDocViewModel.GoogleDocId);
        View.BrowserGrid.Children.Clear();
        View.BrowserGrid.Children.Add(browser);

        ViewModel.DocumentExists = true;
    }

    private ChromiumWebBrowser GetBrowser(string googleDocId) {
        if (_browsers.TryGetValue(googleDocId, out var browser)) {
            return browser;
        }

        var address = $"https://docs.google.com/document/d/{googleDocId}/edit";
        _browsers[googleDocId] = new ChromiumWebBrowser(address);
        return _browsers[googleDocId];
    }

    [Command]
    public async Task CreateDocument() {
        if (_googleDocViewModel == null) {
            return;
        }

        _googleDocViewModel.GoogleDocId = await _googleDocManager.CreateDocument(_googleDocViewModel);
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