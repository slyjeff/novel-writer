using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using Microsoft.Office.Interop.Word;
using Document = Microsoft.Office.Interop.Word.Document;

namespace NovelWriter.Services;


internal sealed class MsWordDocument {
    private readonly Application _word = new();
    private readonly float _pageHeight;
    private readonly Document _document;
    private object _missing = Missing.Value;
    private bool _firstChapter = true;
    private string _title = string.Empty;
    private string _author = string.Empty;

    public MsWordDocument() {
        _document = _word.Documents.Add();
        _pageHeight = 9;

        _document.AutoHyphenation = true;
        _document.PageSetup.DifferentFirstPageHeaderFooter = -1;

        _document.PageSetup.TopMargin = _word.InchesToPoints(0.75f);
        _document.PageSetup.BottomMargin = _word.InchesToPoints(0.75f);
        _document.PageSetup.LeftMargin = _word.InchesToPoints(0.75f);
        _document.PageSetup.RightMargin = _word.InchesToPoints(0.5f);
        _document.PageSetup.Gutter = _word.InchesToPoints(0.13f);
        _document.PageSetup.PageHeight = _word.InchesToPoints(9f);
        _document.PageSetup.PageWidth = _word.InchesToPoints(6f);
        _document.PageSetup.MirrorMargins = -1;

        _document.PageSetup.OddAndEvenPagesHeaderFooter = -1;
    }

    public void SetTitle(string title) {
        _title = title;
    }

    public void SetAuthor(string author) {
        _author = author;
    }

    public void SetTitleFont(string font) {
        var title = _document.Styles.get_Item(WdBuiltinStyle.wdStyleTitle);
        title.Font.Name = font;
        title.Font.Size = 32;
        title.Font.Bold = 1;
        title.ParagraphFormat.Alignment = WdParagraphAlignment.wdAlignParagraphCenter;
        title.NoSpaceBetweenParagraphsOfSameStyle = false;
        title.ParagraphFormat.FirstLineIndent = 0;
    }

    public void SetHeaderFont(string font, int fontSize) {
        var header = _document.Styles.get_Item(WdBuiltinStyle.wdStyleHeader);
        header.Font.Name = font;
        header.Font.Size = fontSize;
        header.Font.Bold = 1;
        header.ParagraphFormat.Alignment = WdParagraphAlignment.wdAlignParagraphCenter;
        header.ParagraphFormat.FirstLineIndent = 0;
        header.ParagraphFormat.SpaceBefore = 0;
        header.ParagraphFormat.SpaceAfter = fontSize;
    }

    public void SetPageNumberFont(string font, int fontSize) {
        var footer = _document.Styles.get_Item(WdBuiltinStyle.wdStyleFooter);
        footer.Font.Name = font;
        footer.Font.Size = fontSize;
        footer.Font.Bold = 1;
        footer.ParagraphFormat.FirstLineIndent = 0;
        footer.ParagraphFormat.SpaceBefore = fontSize;
        footer.ParagraphFormat.SpaceAfter = 0;
    }

    public void SetChapterFont(string font) {
        var chapter = _document.Styles.get_Item(WdBuiltinStyle.wdStyleHeading1);
        chapter.Font.Name = font;
        chapter.Font.Size = 32;
        chapter.Font.Bold = 1;
        chapter.Font.Color = WdColor.wdColorBlack;
        chapter.ParagraphFormat.Alignment = WdParagraphAlignment.wdAlignParagraphCenter;
        chapter.ParagraphFormat.SpaceBefore = _word.InchesToPoints(_pageHeight * 0.30f);
        chapter.ParagraphFormat.SpaceAfter = 26;
        chapter.ParagraphFormat.FirstLineIndent = 0;
    }

    public void SetBodyFont(string font, int fontSize) {
        var normal = _document.Styles.get_Item(WdBuiltinStyle.wdStyleNormal);
        normal.Font.Name = font;
        normal.Font.Size = fontSize;
        normal.Font.Bold = 0;
        normal.Font.Color = WdColor.wdColorBlack;
        normal.ParagraphFormat.Alignment = WdParagraphAlignment.wdAlignParagraphJustify;
        normal.ParagraphFormat.FirstLineIndent = _word.InchesToPoints(0.30f);
        normal.ParagraphFormat.SpaceBefore = 0;
        normal.ParagraphFormat.SpaceAfter = 3;
        normal.ParagraphFormat.LineSpacing = 15;

        var breakStyle = _document.Styles.Add("Break");
        breakStyle.Font.Name = font;
        breakStyle.Font.Size = fontSize;
        breakStyle.Font.Bold = 1;
        breakStyle.Font.Color = WdColor.wdColorBlack;
        breakStyle.ParagraphFormat.Alignment = WdParagraphAlignment.wdAlignParagraphCenter;
        breakStyle.ParagraphFormat.FirstLineIndent = 0;
        breakStyle.ParagraphFormat.SpaceBefore = fontSize;
        breakStyle.ParagraphFormat.SpaceAfter = fontSize;
    }

    public void WriteTitlePage() {
        var title = _document.Content.Paragraphs.Add(ref _missing);
        title.Range.Text = _title;
        title.set_Style(_document.Styles.get_Item(WdBuiltinStyle.wdStyleTitle));
        title.Format.FirstLineIndent = 0;
        title.Range.Font.Size = 40;
        title.Format.SpaceBefore = _word.InchesToPoints(_pageHeight * 0.10f);
        title.Format.SpaceAfter = 0;
        title.Range.InsertParagraphAfter();

        var author = _document.Content.Paragraphs.Add(ref _missing);
        author.Range.Text = _author;
        author.set_Style(_document.Styles.get_Item(WdBuiltinStyle.wdStyleTitle));
        author.Format.FirstLineIndent = 0;
        author.Range.Bold = 0;
        author.Range.Font.Size = 20;
        author.Format.SpaceBefore = _word.InchesToPoints(_pageHeight * 0.4f);
        author.Format.SpaceAfter = 0;
        author.Range.InsertParagraphAfter();
    }

    public void Show() {
        _word.Visible = true;
    }

    public void WriteChapter(string chapterName) {
        var section = _document.Content.Sections.Add();

        var oddHeader = section.Headers[WdHeaderFooterIndex.wdHeaderFooterPrimary];
        oddHeader.Range.Fields.Add(oddHeader.Range, WdFieldType.wdFieldPage);
        oddHeader.Range.set_Style(_document.Styles.get_Item(WdBuiltinStyle.wdStyleHeader));
        oddHeader.Range.ParagraphFormat.Alignment = WdParagraphAlignment.wdAlignParagraphRight;
        oddHeader.Range.Font.Bold = 1;
        oddHeader.Range.Text = _title;

        var evenHeader = section.Headers[WdHeaderFooterIndex.wdHeaderFooterEvenPages];
        evenHeader.Range.Fields.Add(evenHeader.Range, WdFieldType.wdFieldPage);
        evenHeader.Range.set_Style(_document.Styles.get_Item(WdBuiltinStyle.wdStyleHeader));
        evenHeader.Range.ParagraphFormat.Alignment = WdParagraphAlignment.wdAlignParagraphLeft;
        evenHeader.Range.ParagraphFormat.FirstLineIndent = 0;
        evenHeader.Range.Font.Bold = 1;
        evenHeader.Range.Text = _author;

        var footerStyle = _document.Styles.get_Item(WdBuiltinStyle.wdStyleFooter);

        var oddFooter = section.Footers[WdHeaderFooterIndex.wdHeaderFooterPrimary];
        oddFooter.Range.ParagraphFormat.Alignment = WdParagraphAlignment.wdAlignParagraphRight;
        oddFooter.PageNumbers.RestartNumberingAtSection = _firstChapter;
        oddFooter.Range.Font = footerStyle.Font;
        _firstChapter = false;

        oddFooter.PageNumbers.StartingNumber = 1;

        oddFooter.PageNumbers.Add();

        var evenFooter = section.Footers[WdHeaderFooterIndex.wdHeaderFooterEvenPages];
        evenFooter.Range.Font = footerStyle.Font;
        evenFooter.Range.ParagraphFormat.FirstLineIndent = 0;
        evenFooter.Range.ParagraphFormat.Alignment = WdParagraphAlignment.wdAlignParagraphLeft;

        var chapter = _document.Content.Paragraphs.Add(ref _missing);
        chapter.Range.Text = chapterName;
        chapter.set_Style(_document.Styles.get_Item(WdBuiltinStyle.wdStyleHeading1));
        chapter.Format.FirstLineIndent = 0;
        chapter.Range.InsertParagraphAfter();
    }

    public IWordParagraph WriteParagraph() {
        return new WordParagraph(_document);
    }

    public void WriteBreak() {
        var chapter = _document.Content.Paragraphs.Add(ref _missing);
        chapter.Range.Text = "* * *";
        chapter.set_Style(_document.Styles.get_Item("Break"));
        chapter.Range.InsertParagraphAfter();
    }
}


public interface IWordParagraph : IDisposable {
    void AddText(string text, bool? bold, bool? italic);
    void DropCap();
    void CenterJustify();
    void BlockQuote();
}

internal sealed class WordParagraph : IWordParagraph {
    private readonly Document _document;
    private readonly Paragraph _paragraph;
    private readonly StringBuilder _text = new();
    private readonly IList<StyleRange> _styleRanges = new List<StyleRange>();
    private bool _dropCap;
    private bool _centerJustify;
    private bool _blockQuote;

    public WordParagraph(Document document) {
        _document = document;
        _paragraph = document.Paragraphs.Add();
    }

    private readonly struct StyleRange {
        public int Start { get; }
        public int End { get; }
        public bool Bold { get; }
        public bool Italic { get; }

        public StyleRange(int start, int end, bool bold, bool italic) {
            Start = start;
            End = end;
            Bold = bold;
            Italic = italic;
        }
    }

    public void AddText(string text, bool? bold, bool? italic) {
        var start = _text.Length;
        _text.Append(text);
        if (bold == true || italic == true) {
            _styleRanges.Add(new StyleRange(start - 1, _text.Length - 1, bold == true, italic == true));
        }
    }

    public void DropCap() {
        _dropCap = true;
    }

    public void CenterJustify() {
        _centerJustify = true;
    }

    public void BlockQuote() {
        _blockQuote = true;
    }

    public void Dispose() {
        var text = _text.ToString();
        if (text.EndsWith("\n")) {
            text = text[..^1];
        }

        if (_dropCap && text.StartsWith("“")) {
            text = text[1..];
        }

        _paragraph.Range.Text = text;
        _paragraph.set_Style(_document.Styles.get_Item(WdBuiltinStyle.wdStyleNormal));

        if (_dropCap) {
            _paragraph.FirstLineIndent = 0;
            var dropCap = _paragraph.DropCap;
            dropCap.Position = WdDropPosition.wdDropNormal;
            dropCap.LinesToDrop = 2;
            dropCap.DistanceFromText = 1.5f;
        }

        if (_centerJustify) {
            _paragraph.Format.Alignment = WdParagraphAlignment.wdAlignParagraphCenter;
        }

        if (_blockQuote) {
            _paragraph.LeftIndent = 36;
            _paragraph.RightIndent = 36;
            _paragraph.FirstLineIndent = 0;
        }

        foreach (var styleRange in _styleRanges) {
            var range = _document.Range(_paragraph.Range.Start + styleRange.Start, _paragraph.Range.Start + styleRange.End);
            range.Bold = styleRange.Bold ? 1 : 0;
            range.Italic = styleRange.Italic ? 1 : 0;
        }

        _paragraph.Range.InsertParagraphAfter();
    }
}
