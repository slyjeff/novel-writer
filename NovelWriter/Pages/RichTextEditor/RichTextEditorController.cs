using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using NovelWriter.PageControls;
using NovelWriter.Services;

namespace NovelWriter.Pages.RichTextEditor;

public interface IRichTextEditorController {
    RichTextEditorView View { get; }
    void Show(IRichTextViewModel richTextViewModel);
}

internal sealed class RichTextEditorController : Controller<RichTextEditorView, RichTextEditorViewModel>, IRichTextEditorController {
    private IRichTextViewModel? _richTextViewModel;
    private bool _loading;
    
    public RichTextEditorController(IDataPersister dataPersister) {
        View.RichTextBox.TextChanged += async (_, _) => {
            if (_richTextViewModel == null || _loading) {
                return;
            }
            
            _richTextViewModel.RichText = GetTextFromRichTextBox(View.RichTextBox);
            await dataPersister.Save();
        };
    }
    
    public void Show(IRichTextViewModel richTextViewModel) {
        try {
            _loading = true;
            View.RichTextBox.Document.Blocks.Clear();

            _richTextViewModel = richTextViewModel;
            if (_richTextViewModel.RichText == string.Empty) {
                return;
            }

            using (var ms = new MemoryStream(Encoding.UTF8.GetBytes(_richTextViewModel.RichText))) {
                View.RichTextBox.Selection.Load(ms, DataFormats.Rtf);
            }
        } finally {
            _loading = false;
        }
    }
    
    private static string GetTextFromRichTextBox(RichTextBox richTextBox) {
        var textRange = new TextRange(richTextBox.Document.ContentStart, richTextBox.Document.ContentEnd);
        using (var memory = new MemoryStream()) {
            textRange.Save(memory, DataFormats.Rtf);
            return Encoding.UTF8.GetString(memory.ToArray());
        }
    }
}
