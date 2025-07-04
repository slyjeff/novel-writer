﻿using NovelDocs.PageControls;

namespace NovelDocs.Pages.CompileStatus; 

public class CompileStatusViewModel : ViewModel {
    public virtual int Progress { get; set; }
    public virtual int Max { get; set; }
    public virtual string Chapter { get; set; } = string.Empty;
}