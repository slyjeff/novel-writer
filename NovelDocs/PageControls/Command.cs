using System;
using System.Threading.Tasks;
using System.Windows.Input;

namespace NovelDocs.PageControls; 

internal sealed class Command : ICommand {
    private readonly object _controller;
    private readonly string _methodName;

    public Command(object controller, string methodName) {
        _controller = controller;
        _methodName = methodName;
    }

    public bool CanExecute(object? parameter) {
        return true;
    }

    public event EventHandler? CanExecuteChanged;

    public void Execute(object? parameter) {
        var method = _controller.GetType().GetMethod(_methodName);
        if (method == default) {
            return;
        }

        var parameters = (method.GetParameters().Length == 1) ? new[] { parameter } : Array.Empty<object>();
        method.Invoke(_controller, parameters);
    }
}

internal sealed class CommandAsync : ICommand {
    private readonly object _controller;
    private readonly string _methodName;

    public CommandAsync(object controller, string methodName) {
        _controller = controller;
        _methodName = methodName;
    }

    public bool CanExecute(object? parameter) {
        return true;
    }

    public event EventHandler? CanExecuteChanged;

    public async void Execute(object? parameter) {
        var method = _controller.GetType().GetMethod(_methodName);
        if (method == default) {
            return;
        }

        var parameters = (method.GetParameters().Length == 1) ? new[] { parameter } : Array.Empty<object>();
        if (method.Invoke(_controller, parameters) is Task task) {
            await task;
        }
    }
}