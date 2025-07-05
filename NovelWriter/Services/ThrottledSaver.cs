using System;
using System.Threading;
using System.Threading.Tasks;
using Timer = System.Timers.Timer;

namespace NovelWriter.Services;

public interface IThrottledSaver : IDisposable {
    Task SetSaveAction(Func<Task> saveAction);
    Task SetMinInterval(TimeSpan minInterval);
    Task Save();
    Task FlushPendingChanges();
}

internal sealed class ThrottledSaver : IThrottledSaver {
    private readonly Timer _timer; 
    private readonly SemaphoreSlim _semaphore = new(1);
    private bool _hasPendingChanges;
    private bool _isSaving;

    public ThrottledSaver() {
        _timer = new Timer(_minInterval.TotalMilliseconds) { AutoReset = false };
        _timer.Elapsed += async (_, _) => await HandleTimerElapsed();
    }

    private Func<Task> _saveAction = () => Task.CompletedTask;
    public async Task SetSaveAction(Func<Task> saveAction) {
        await FlushPendingChanges();
        _saveAction = saveAction;
    }

    private TimeSpan _minInterval = TimeSpan.FromSeconds(5);
    public async Task SetMinInterval(TimeSpan minInterval) {
        _minInterval = minInterval;
        _timer.Interval = minInterval.TotalMilliseconds;
        await FlushPendingChanges();
    }
    
    public async Task Save() {
        await _semaphore.WaitAsync();
        try {
            if (_isSaving) {
                _hasPendingChanges = true;
                return;
            }

            if (!_timer.Enabled) {
                _isSaving = true;
                await _saveAction();
                _isSaving = false;
                
                _timer.Start();
            } else {
                _hasPendingChanges = true;
            }
        } finally {
            _semaphore.Release();
        }
    }

    public async Task FlushPendingChanges() {
        await _semaphore.WaitAsync();
        try {
            if (!_hasPendingChanges) {
                return;
            }

            _hasPendingChanges = false;
            _timer.Stop();
            await _saveAction();
            _timer.Start();
        } finally {
            _semaphore.Release();
        }
    }

    private async Task HandleTimerElapsed() {
        await _semaphore.WaitAsync();
        try {
            if (!_hasPendingChanges) {
                return;
            }

            _hasPendingChanges = false;
            _isSaving = true;
            await _saveAction();
            _isSaving = false;
            _timer.Start();
        } finally {
            _semaphore.Release();
        }
    }

    public void Dispose() {
        _timer.Dispose();
        _semaphore.Dispose();
    }
}
