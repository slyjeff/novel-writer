using System;
using CefSharp;
using CefSharp.Wpf;
using NovelDocs.PageControls;
using static System.Net.WebRequestMethods;

namespace NovelDocs.Pages.GoogleDoc;

public interface IGoogleDocController {
    GoogleDocView View { get; }
    void Show(Action<string> assignDocument);
    void Hide();
    void BrowseToDoc(string googleDocId);
}

internal sealed class GoogleDocController : Controller<GoogleDocView, GoogleDocViewModel>, IGoogleDocController {
    private readonly ChromiumWebBrowser _browser;
    private Action<string> _assignDocument = null!; //will be assigned whenever the form is made visible

    public GoogleDocController() {
        var settings = new CefSettings {
            UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/96.0.4664.110 Safari/537.36 /CefSharp Browser" + Cef.CefSharpVersion,
            CachePath = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location)
        };
        Cef.Initialize(settings);

        _browser = new ChromiumWebBrowser();

        View.BrowserGrid.Children.Add(_browser);
        //chromiumBrowser.Address = "https://docs.google.com/document/d/1MrZwrRFvqTE_3k6GGD-K6JeoMj5HEi5ClzomlB29XcM/edit";
    }

    public void Show(Action<string> assignDocument) {
        _assignDocument = assignDocument;
        ViewModel.IsVisible = true;
    }

    public void Hide() {
        ViewModel.IsVisible = false;
    }

    public void BrowseToDoc(string googleDocId) {
        if (string.IsNullOrEmpty(googleDocId)) {
            ViewModel.DocumentExists = false;
            return;
        }

        _browser.Address = $"https://docs.google.com/document/d/{googleDocId}/edit";
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
    public void AssigningExistingDocumentConfirmed() {
        _assignDocument(ViewModel.GoogleDocId);
        ViewModel.AssigningExistingDocument = false;
    }
}