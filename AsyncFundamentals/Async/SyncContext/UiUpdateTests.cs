namespace AsyncFundamentals.Async.SyncContext;

public class UiUpdateTests
{
    [Test]
    public void UpdateUiContent_ContinueOnCapturedContext_Updated()
    {
        const string UpdatedContent = nameof(UpdateUiContent_ContinueOnCapturedContext_Updated);
        SynchronizationContext.SetSynchronizationContext(Fixture.UiContext);
        $"Synchronization context set: {SynchronizationContext.Current}".ThreadDump();
        var updateTask = AwaitTaskUpdateUi(UpdatedContent, true);
        Assert.Multiple(
            () =>
            {
                Assert.DoesNotThrow(updateTask.Wait);
                Assert.That(Fixture.UiContext.UiContent, Is.EqualTo(UpdatedContent));
                Fixture.AssertUiHeartBeat();
            });
    }

    [Test]
    public void UpdateUiContent_NotContinueOnCapturedContext_Exception()
    {
        const string UpdatedContent = nameof(UpdateUiContent_NotContinueOnCapturedContext_Exception);
        SynchronizationContext.SetSynchronizationContext(Fixture.UiContext);
        $"Synchronization context set: {SynchronizationContext.Current}".ThreadDump();
        var updateTask = AwaitTaskUpdateUi(UpdatedContent, false);
        Assert.Multiple(
            () =>
            {
                var exception = Assert.Throws<AggregateException>(updateTask.Wait);
                Assert.That(exception!.InnerException, Is.TypeOf<InvalidOperationException>());
                Assert.That(exception.InnerException!.Message, Is.EqualTo("Only UI thread can update UI content"));
                Assert.That(Fixture.UiContext.UiContent, Is.Not.EqualTo(UpdatedContent));
                Fixture.AssertUiHeartBeat();
            });
    }

    static async Task AwaitTaskUpdateUi(string updatedContent, bool continueOnCapturedContext)
    {
        $"Awaiting task ({continueOnCapturedContext})".ThreadDump();
        Fixture.UiContext.UiContent = await Task.Run(() => updatedContent).ConfigureAwait(continueOnCapturedContext);
        $"Done await({continueOnCapturedContext})".ThreadDump();
    }
}
