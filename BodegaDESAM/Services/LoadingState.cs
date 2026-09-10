namespace BodegaDESAM.Services;

/// <summary>
/// Tracks asynchronous operations for the current Blazor circuit.
/// </summary>
public sealed class LoadingState
{
    private int _activeOperations;

    public event Action? Changed;

    public bool IsLoading => Volatile.Read(ref _activeOperations) > 0;

    public IDisposable Begin()
    {
        Interlocked.Increment(ref _activeOperations);
        Changed?.Invoke();
        return new LoadingScope(this);
    }

    private void End()
    {
        var remaining = Interlocked.Decrement(ref _activeOperations);
        if (remaining < 0)
        {
            Interlocked.Exchange(ref _activeOperations, 0);
        }

        Changed?.Invoke();
    }

    private sealed class LoadingScope : IDisposable
    {
        private LoadingState? _owner;

        public LoadingScope(LoadingState owner)
        {
            _owner = owner;
        }

        public void Dispose()
        {
            Interlocked.Exchange(ref _owner, null)?.End();
        }
    }
}
