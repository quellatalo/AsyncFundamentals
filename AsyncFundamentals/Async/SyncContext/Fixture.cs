namespace AsyncFundamentals.Async.SyncContext;

[SetUpFixture]
public static class Fixture
{
    public static readonly SynchronizationContext DefaultContext = new();
    static UiContext? s_uiContext;

    public static UiContext UiContext => s_uiContext!;

    [OneTimeSetUp]
    public static void OneTimeSetUp()
    {
        "Synchronization context: {SynchronizationContext.Current}".ThreadDump();
        s_uiContext = UiContext.Start();
        "Created context: {s_uiContext}".ThreadDump();
    }

    [OneTimeTearDown]
    public static void OneTimeTearDown() => UiContext.Finish();

    public static void AssertUiHeartBeat(bool isStable = true)
    {
        int highBoundary = UiContext.UiUpdateInterval + 100;
        Assert.That(
            UiContext.UiHeartBeatHistory,
            isStable ? Has.All.LessThanOrEqualTo(highBoundary) : Has.Some.GreaterThan(highBoundary));
    }
}
