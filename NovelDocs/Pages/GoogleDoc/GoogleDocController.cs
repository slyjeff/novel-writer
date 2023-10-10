using System;
using System.Threading.Tasks;
using CefSharp;
using CefSharp.Wpf;
using NovelDocs.PageControls;
using NovelDocs.Services;

namespace NovelDocs.Pages.GoogleDoc;

public interface IGoogleDocController {
    GoogleDocView View { get; }
    Task Show(string googleDocId, Action<string> assignDocument);
    void Hide();
}

internal sealed class GoogleDocController : Controller<GoogleDocView, GoogleDocViewModel>, IGoogleDocController {
    private readonly IGoogleDocService _googleDocService;
    private readonly ChromiumWebBrowser _browser;
    private Action<string> _assignDocument = null!; //will be assigned whenever the form is made visible

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

    public async Task Show(string googleDocId, Action<string> assignDocument) {
        _assignDocument = assignDocument;
        await BrowseToDoc(googleDocId);
        ViewModel.IsVisible = true;
    }

    public void Hide() {
        ViewModel.IsVisible = false;
    }

    private async Task BrowseToDoc(string googleDocId) {
        if (!await _googleDocService.GoogleDocExists(googleDocId)) {
            ViewModel.DocumentExists = false;
            return;
        }

        var address = $"https://docs.google.com/document/d/{googleDocId}/edit";
        if (_browser.Address == null || !_browser.Address.StartsWith(address)) {
            _browser.Address = address;
        }

        ViewModel.DocumentExists = true;
    }

    [Command]
    public void CreateDocument() {
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
    public async Task AssigningExistingDocumentConfirmed() {
        _assignDocument(ViewModel.GoogleDocId);
        await BrowseToDoc(ViewModel.GoogleDocId);
        ViewModel.AssigningExistingDocument = false;
    }
}