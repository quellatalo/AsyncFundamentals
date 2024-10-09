namespace AsyncFundamentals.Async;

public class WhenAllTests
{
    TaskStatus[]? _taskStatuses;

    [TestCase(10)]
    public void WhenAll_ThrowFirstInnerException(int taskCount)
    {
        _taskStatuses = new TaskStatus[taskCount];
        var tasks = Enumerable.Range(0, taskCount).Select(i => DoSomeThingAsync(i, (i & 1) == 1));
        var whenAllTask = Task.WhenAll(tasks);
        Assert.Multiple(
            () =>
            {
                // throws the first inner exception from the WhenAll task.
                var thrownException =
                    Assert.ThrowsAsync<InvalidOperationException>(async () => await whenAllTask.ConfigureAwait(false));
                Assert.That(
                    thrownException,
                    Is.EqualTo(whenAllTask.Exception!.InnerExceptions.First())
                        .And.EqualTo(whenAllTask.Exception!.InnerException));

                // the actual WhenAll task's exception is AggregationException,
                // which has an InnerExceptions that represents a collection of exceptions occured from the inner tasks.
                Assert.That(whenAllTask.Exception, Is.TypeOf<AggregateException>());
                Assert.That(whenAllTask.Exception.InnerExceptions, Has.All.TypeOf<InvalidOperationException>());
                Assert.That(whenAllTask.Exception.InnerExceptions, Has.Count.EqualTo(taskCount >> 1));

                // Throwing/catching the WhenAll task's exception does not terminate the inner tasks.
                Assert.That(_taskStatuses, Has.All.EqualTo(TaskStatus.Faulted).Or.EqualTo(TaskStatus.RanToCompletion));
            });
    }

    async Task DoSomeThingAsync(int id, bool throwException = false)
    {
        ArgumentNullException.ThrowIfNull(_taskStatuses);
        $"{id} started".Dump();
        _taskStatuses[id] = TaskStatus.Running;
        await Task.Delay(1000).ConfigureAwait(false);
        if (throwException)
        {
            _taskStatuses[id] = TaskStatus.Faulted;
            $"{id} has failed".Dump();
            throw new InvalidOperationException();
        }

        _taskStatuses[id] = TaskStatus.RanToCompletion;
        $"{id} completed".Dump();
    }
}
