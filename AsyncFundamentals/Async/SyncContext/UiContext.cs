using System.Diagnostics;

namespace AsyncFundamentals.Async.SyncContext;

/// <summary>
/// A customized <see cref="SynchronizationContext"/> that contains a UI thread.
/// This context will synchronize actions with the UI thread.
/// </summary>
public class UiContext : SynchronizationContext
{
    public const int HeartBeatHistoryCount = 20;
    readonly Queue<(SendOrPostCallback SendOrPostCallBack, object? State)> _actionQueue = new();
    readonly Thread _uiThread;
    string _content = string.Empty;

    UiContext(int uiUpdateInterval)
    {
        UiUpdateInterval = uiUpdateInterval;
        _uiThread = new Thread(MockUiJob);
    }

    /// <summary>
    /// Gets the history of the actual UI update intervals.
    /// </summary>
    public History<long> UiHeartBeatHistory { get; } = new(HeartBeatHistoryCount);

    /// <summary>
    /// Gets or sets the UI content, which is reserved exclusively for UI thread to update.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    /// Occurs when a different thread attempts to update the UI content.
    /// </exception>
    public string UiContent
    {
        get => _content;
        set
        {
            if (Environment.CurrentManagedThreadId != UiThread)
            {
                throw new InvalidOperationException("Only UI thread can update UI content");
            }

            _content = value;
        }
    }

    /// <summary>
    /// Gets a value indicating whether the underlying UI thread is active.
    /// </summary>
    public bool Active { get; private set; }

    /// <summary>
    /// Gets a value indicating whether the tasks are running.
    /// </summary>
    public bool Running => _actionQueue.Count != 0;

    /// <summary>
    /// Gets the underlying UI thread ID.
    /// </summary>
    public int UiThread => _uiThread.ManagedThreadId;

    /// <summary>
    /// Gets the UI update interval in milliseconds.
    /// </summary>
    public int UiUpdateInterval { get; }

    /// <summary>
    /// Creates a <see cref="UiContext"/> with an active UI thread.
    /// </summary>
    /// <param name="triggerInterval">Ui updated interval.</param>
    /// <returns>A <see cref="UiContext"/> with an active UI thread.</returns>
    public static UiContext Start(int triggerInterval = 500)
    {
        UiContext ctx = new(triggerInterval) { Active = true };
        ctx._uiThread.Start();
        return ctx;
    }

    /// <inheritdoc/>
    public override void Post(SendOrPostCallback d, object? state) => _actionQueue.Enqueue((d, state));

    /// <inheritdoc/>
    public override string ToString()
        => $"{nameof(UiContext)} created under thread {Environment.CurrentManagedThreadId}" +
           $", with underlying UI thread {UiThread}";

    /// <summary>
    /// Signals the UI thread to stop, and wait for all queued tasks to finish.
    /// </summary>
    /// <param name="timeout">Timeout for waiting.</param>
    /// <exception cref="TimeoutException">Occurs when queued tasks are not finished after timeout.</exception>
    public void Finish(int timeout = 3000)
    {
        Active = false;
        if (!_uiThread.Join(timeout))
        {
            throw new TimeoutException("Timed out waiting for UI thread to finish");
        }
    }

    /// <summary>
    /// Waits for all queued tasks to finish.
    /// </summary>
    public void Wait()
    {
        do
        {
            Thread.Sleep(UiUpdateInterval);
        }
        while (Running);
    }

    void MockUiJob()
    {
        var stopwatch = Stopwatch.StartNew();
        while (Active || Running)
        {
            long heartBeat = stopwatch.ElapsedMilliseconds;
            UiHeartBeatHistory.Add(heartBeat);
            $"UI heartbeat after {heartBeat} ms".ThreadDump();
            stopwatch.Restart();
            if (_actionQueue.TryDequeue(out var callback))
            {
                callback.SendOrPostCallBack(callback.State);
            }

            Thread.Sleep(UiUpdateInterval);
        }
    }
}
