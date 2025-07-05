using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using NovelWriter.PageControls;
using NovelWriter.Services;

namespace NovelWriter.Pages.RichTextEditor;

public interface IRichTextEditorController: IAsyncDisposable {
    RichTextEditorView View { get; }
    Task Show(IRichTextViewModel richTextViewModel);
}

internal sealed class RichTextEditorController : Controller<RichTextEditorView, RichTextEditorViewModel>, IRichTextEditorController {
    private readonly IDataPersister _dataPersister;
    private readonly IThrottledSaver _throttledSaver;
    private IRichTextViewModel? _richTextViewModel;
    private bool _loading;
    
    public RichTextEditorController(IDataPersister dataPersister, IThrottledSaver throttledSaver) {
        _dataPersister = dataPersister;
        _throttledSaver = throttledSaver;
        View.RichTextBox.TextChanged += async (_, _) => {
            if (_richTextViewModel == null || _loading) {
                return;
            }

            await throttledSaver.Save();
        };
    }
    
    public async Task Show(IRichTextViewModel richTextViewModel) {
        await _throttledSaver.SetSaveAction(async () => {
            var content = GetTextFromRichTextBox(View.RichTextBox);
            await _dataPersister.SaveDocumentContent(richTextViewModel.DocumentOwner, content);
        });
        
        _richTextViewModel = richTextViewModel;

        try {
            _loading = true;
            View.RichTextBox.Document.Blocks.Clear();

            var content = await _dataPersister.GetDocumentContent(_richTextViewModel.DocumentOwner);
            if (content == string.Empty) {
                return;
            }

            using (var ms = new MemoryStream(Encoding.UTF8.GetBytes(content))) {
                View.RichTextBox.Selection.Load(ms, DataFormats.Rtf);
            }
        } finally {
            _loading = false;
        }
    }
    
    private static string GetTextFromRichTextBox(RichTextBox richTextBox) {
        return richTextBox.Dispatcher.CheckAccess() 
            ? GetTextFromRichTextBoxInternal(richTextBox) 
            : richTextBox.Dispatcher.Invoke(() => GetTextFromRichTextBoxInternal(richTextBox));
    }

    private static string GetTextFromRichTextBoxInternal(RichTextBox richTextBox) {
        var textRange = new TextRange(richTextBox.Document.ContentStart, richTextBox.Document.ContentEnd);
        using (var memory = new MemoryStream()) {
            textRange.Save(memory, DataFormats.Rtf);
            return Encoding.UTF8.GetString(memory.ToArray());
        }
    }


    public async ValueTask DisposeAsync() {
        await _throttledSaver.FlushPendingChanges();
    }
}
