using NovelWriter.PageControls;

namespace NovelWriter.Pages.CompileStatus;

public interface ICompileStatusController {
    CompileStatusView View { get; }
    CompileStatusViewModel ViewModel { get; }
}

internal sealed class CompileStatusController : Controller<CompileStatusView, CompileStatusViewModel>, ICompileStatusController {
}