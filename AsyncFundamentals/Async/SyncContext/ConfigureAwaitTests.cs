using Newtonsoft.Json;

namespace AsyncFundamentals.Async.SyncContext;

public class ConfigureAwaitTests
{
    int _continuedThread;

    [Test]
    public void DefaultContext_Fast_UnableToContinueCapturedContext(
        [Values] bool setContext,
        [Values] bool continueOnCapturedContext)
    {
        if (setContext)
        {
            SynchronizationContext.SetSynchronizationContext(Fixture.DefaultContext);
        }

        $"{_continuedThread}: Synchronization context: {SynchronizationContext.Current}".Dump();
        AwaitTaskThatPrintsRunningThreadAsync(continueOnCapturedContext).Wait();
        Fixture.UiContext.Wait();
        Assert.That(_continuedThread, Is.Not.EqualTo(Fixture.UiContext.UiThread));
        Fixture.AssertUiHeartBeat();
    }

    [Test]
    public void SyncContext_Fast_ContinueOnCapturedContext([Values] bool continueOnCapturedContext)
    {
        SynchronizationContext.SetSynchronizationContext(Fixture.UiContext);
        $"{_continuedThread}: Synchronization context set: {SynchronizationContext.Current}".Dump();
        AwaitTaskThatPrintsRunningThreadAsync(continueOnCapturedContext).Wait();
        Fixture.UiContext.Wait();
        Assert.That(
            _continuedThread,
            continueOnCapturedContext
                ? Is.EqualTo(Fixture.UiContext.UiThread)
                : Is.Not.EqualTo(Fixture.UiContext.UiThread));
        Fixture.AssertUiHeartBeat();
    }

    [Test]
    public void SyncContext_Slow_ContinueOnCapturedContext([Values] bool continueOnCapturedContext)
    {
        const int SlownessMilliseconds = 1000;
        SynchronizationContext.SetSynchronizationContext(Fixture.UiContext);
        $"{_continuedThread}: Synchronization context set: {SynchronizationContext.Current}".Dump();
        AwaitTaskThatPrintsRunningThreadAsync(continueOnCapturedContext, SlownessMilliseconds).Wait();
        Fixture.UiContext.Wait();
        Assert.That(
            _continuedThread,
            continueOnCapturedContext
                ? Is.EqualTo(Fixture.UiContext.UiThread)
                : Is.Not.EqualTo(Fixture.UiContext.UiThread));
        JsonConvert.SerializeObject(Fixture.UiContext.UiHeartBeatHistory).Dump();
        Fixture.AssertUiHeartBeat(!continueOnCapturedContext);
        Fixture.UiContext.UiHeartBeatHistory.Clear();
    }

    static void BusyOperation(int milliseconds) => Thread.Sleep(milliseconds);

    async Task AwaitTaskThatPrintsRunningThreadAsync(
        bool continueOnCapturedContext,
        int slownessMilliseconds = 0,
        CancellationToken cancellationToken = default)
    {
        $"Awaiting task ({continueOnCapturedContext})".ThreadDump();
        await Task.Run(() => { "Task executing".ThreadDump(); }, cancellationToken)
            .ConfigureAwait(continueOnCapturedContext);
        _continuedThread = Environment.CurrentManagedThreadId;
        $"{_continuedThread}: Done await({continueOnCapturedContext})".Dump();
        BusyOperation(slownessMilliseconds);
    }
}
