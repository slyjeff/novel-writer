using System.Threading.Tasks;
using NovelDocs.Entity;
using NovelDocs.Services;

namespace NovelDocs.Managers;

public interface IMsWordManager {
    Task Compile();
}

internal sealed class MsWordManager : IMsWordManager {
    private readonly IDataPersister _dataPersister;
    private readonly IGoogleDocService _googleDocService;
    private readonly ICompileStatusService _compileStatusService;

    public MsWordManager(IDataPersister dataPersister, IGoogleDocService googleDocService, ICompileStatusService compileStatusService) {
        _dataPersister = dataPersister;
        _googleDocService = googleDocService;
        _compileStatusService = compileStatusService;
    }

    public async Task Compile() {
        var novel = _dataPersister.GetLastOpenedNovel();
        if (novel == null) {
            return;
        }

        var docIds = novel.ManuscriptElements.GetDocIds();

        var cancelCompile = false;
        using (var compileStatus = _compileStatusService.ShowCompileStatus(docIds.Count, () => cancelCompile = true)) {

            var document = new MsWordDocument(novel);
            document.WriteTitlePage();

            var writeBreak = false;
            var dropCap = true;

            var progress = 0;
            foreach (var docId in docIds) {
                if (cancelCompile) {
                    break;
                }

                compileStatus.UpdateProgress(progress++);

                var isNewChapter = docId.StartsWith("Chapter:");
                if (isNewChapter) {
                    var chapter = docId["Chapter:".Length..];
                    compileStatus.UpdateChapter(chapter);
                    document.WriteChapter(chapter);
                    writeBreak = false;
                    dropCap = true;
                    continue;
                }

                var googleDoc = await _googleDocService.GetGoogleDoc(docId);
                if (googleDoc == null) {
                    continue;
                }

                if (writeBreak) {
                    document.WriteBreak();
                }

                foreach (var contentItem in googleDoc.Body.Content) {
                    if (contentItem.Paragraph == null) {
                        continue;
                    }

                    using (var paragraph = document.WriteParagraph()) {
                        var blockQuote = false;

                        if (contentItem.Paragraph.ParagraphStyle != null) {
                            if (contentItem.Paragraph.ParagraphStyle.Alignment == "CENTER") {
                                paragraph.CenterJustify();
                            }

                            if (contentItem.Paragraph.ParagraphStyle.IndentStart != null && contentItem.Paragraph.ParagraphStyle.IndentEnd != null) {
                                paragraph.BlockQuote();
                                blockQuote = true;
                            }
                        }

                        foreach (var element in contentItem.Paragraph.Elements) {
                            if (string.IsNullOrEmpty(element.TextRun?.Content) || element.TextRun.Content == "\n") {
                                continue;
                            }

                            if (!blockQuote && dropCap) {
                                paragraph.DropCap();
                                dropCap = false;
                            }

                            paragraph.AddText(element.TextRun.Content, element.TextRun.TextStyle.Bold, element.TextRun.TextStyle.Italic);
                        }
                    }

                    writeBreak = true;
                }

            }
            document.Show();
        }
    }
}