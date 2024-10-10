#pragma warning disable CA1849 // Call async methods when in an async method
#pragma warning disable CA2007 // Do not directly await a Task
using System.Diagnostics;

namespace AsyncFundamentals.Async.Await;

public class AwaitTests
{
    const int TaskCount = 10;
    const int DelayMs = 300;
    static readonly List<EventRecord> s_events = [];

    [SetUp]
    public void Setup()
    {
        s_events.Clear();
        EventRecord.RestartWatch();
    }

    [Test]
    public void UseAwait_Async([Values] bool useAwait)
    {
        var tasks = new Task<long>[TaskCount];
        for (int i = 0; i < TaskCount; i++)
        {
            tasks[i] = DoJob(i, useAwait);
        }

        Task.WhenAll(tasks).Wait();
        s_events.Add(new EventRecord(int.MaxValue, TaskStatus.RanToCompletion));
        Assert.That(s_events, useAwait ? Is.Not.Ordered : Is.Ordered);
        EventRecord.EventsToString(s_events).Dump();
    }

    static async Task<long> DoJob(int jobId, bool useAwait)
    {
        var task = Task.Run(
            () =>
            {
                s_events.Add(new EventRecord(jobId));
                return TimeConsumingJob();
            });
        long timeConsumed = useAwait ? await task : task.Result;
        s_events.Add(new EventRecord(jobId, TaskStatus.RanToCompletion));
        return timeConsumed;
    }

    /// <summary>
    /// Synchronous job that takes <see cref="DelayMs"/> time on the current thread, and returns its time consumed.
    /// </summary>
    /// <returns>Time consumed.</returns>
    static long TimeConsumingJob()
    {
        var stopwatch = Stopwatch.StartNew();
        Thread.Sleep(DelayMs);
        return stopwatch.ElapsedMilliseconds;
    }
}
