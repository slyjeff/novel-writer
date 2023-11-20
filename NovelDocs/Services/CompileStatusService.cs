using System;
using System.Threading;
using System.Windows;
using System.Windows.Threading;
using Microsoft.Extensions.DependencyInjection;
using NovelDocs.Pages.CompileStatus;

namespace NovelDocs.Services;

public interface ICompileStatus : IDisposable {
    void UpdateProgress(int newValue);
    void UpdateChapter(string chapter);
}

public interface ICompileStatusService {
    ICompileStatus ShowCompileStatus(int max, Action cancelAction);
}

internal sealed class CompileStatusService : ICompileStatusService {
    private readonly IServiceProvider _serviceProvider;

    public CompileStatusService(IServiceProvider serviceProvider) {
        _serviceProvider = serviceProvider;
    }

    public ICompileStatus ShowCompileStatus(int max, Action cancelAction) {
        var controller = _serviceProvider.GetService<ICompileStatusController>();
        if (controller == null) {
            throw new Exception("Could not resolved 'ICompileStatusController'");
        }
        controller.ViewModel.Max = max;
        return new CompileStatus(controller, cancelAction);
    }

    private sealed class CompileStatus : ICompileStatus {
        private readonly ICompileStatusController _compileStatusController;
        private readonly Action _cancelAction;

        public CompileStatus(ICompileStatusController compileStatusController, Action cancelAction) {
            _compileStatusController = compileStatusController;
            _cancelAction = cancelAction;
            _compileStatusController.View.Owner = Application.Current.MainWindow;

            _compileStatusController.View.Show();
            _compileStatusController.ViewModel.Chapter = "Starting Compile . . .";
            _compileStatusController.View.Closed += ViewClosed;
        }

        private void ViewClosed(object? sender, EventArgs e) {
            _cancelAction.Invoke();
        }

        public void UpdateProgress(int newValue) {
            _compileStatusController.ViewModel.Progress = newValue;
        }

        public void UpdateChapter(string chapter) {
            _compileStatusController.ViewModel.Chapter = chapter;
        }

        public void Dispose() {
            _compileStatusController.View.Closed -= ViewClosed;
            _compileStatusController.View.Close();
        }
    }
}

