The usage of `await` compared to `.Wait()` or `.Result`
=======================================================

Short answer:
- `await` is an `async` operation
- `.Wait()` and `.Result` are not

But what does that actually mean?

To demonstrate, we execute the test [AwaitTests.UseAwait_Async](AwaitTests.cs#L21).

It would run 2 cases:
- `UseAwait_Async(False)`: use `.Result` to retrieve data
- `UseAwait_Async(True)`: use `await` to retrieve data

# The scenario

To make it simple, `UseAwait_Async` is just a plain `void` method, without `async` keyword.
So, it does not do anything async on by itself. Everything is linear to observe.

`UseAwait_Async` would run 10 [DoJob](AwaitTests.cs#L35) tasks, with IDs from 0-9.
The main test would take the ID of `2147483647`.

The `DoJob` task has `async` keyword so it has the capability to utilize `async` operations.
Its main feature is to record when it started, when it is completed, and return the time consumed.

When the input parameter `useAwait` is `true`, `DoJob` would run its task with `await` keyword.
And when `useAwait` is `false`, it would use `.Result`.

In the end, we print out all the events recorded to observe their chronological order.

_`DoJob` when run alone by itself would have no difference whether `useAwait` is true or false.
However, let's see how it would affect its caller `UseAwait_Async`._

## Async case: `UseAwait_Async(True)`

In this case, `DoJob` would use `await`.

The output:
```text
EventRecord { Id = 0, Status = Running, TimeMs = 1 }
EventRecord { Id = 1, Status = Running, TimeMs = 2 }
EventRecord { Id = 2, Status = Running, TimeMs = 3 }
EventRecord { Id = 3, Status = Running, TimeMs = 3 }
EventRecord { Id = 4, Status = Running, TimeMs = 3 }
EventRecord { Id = 5, Status = Running, TimeMs = 4 }
EventRecord { Id = 6, Status = Running, TimeMs = 4 }
EventRecord { Id = 7, Status = Running, TimeMs = 4 }
EventRecord { Id = 8, Status = Running, TimeMs = 5 }
EventRecord { Id = 9, Status = Running, TimeMs = 5 }
EventRecord { Id = 0, Status = RanToCompletion, TimeMs = 302 }
EventRecord { Id = 2, Status = RanToCompletion, TimeMs = 303 }
EventRecord { Id = 1, Status = RanToCompletion, TimeMs = 303 }
EventRecord { Id = 7, Status = RanToCompletion, TimeMs = 304 }
EventRecord { Id = 3, Status = RanToCompletion, TimeMs = 304 }
EventRecord { Id = 5, Status = RanToCompletion, TimeMs = 304 }
EventRecord { Id = 4, Status = RanToCompletion, TimeMs = 304 }
EventRecord { Id = 8, Status = RanToCompletion, TimeMs = 305 }
EventRecord { Id = 9, Status = RanToCompletion, TimeMs = 305 }
EventRecord { Id = 2147483647, Status = RanToCompletion, TimeMs = 306 }
```

As observed in the output, all the DoJob tasks were able to get started at almost the same time.
And they asynchronously ran toward completion (in unpredictable manner).

In the end, we completed 10 tasks, each took about 300ms in a total of just about 300ms.

## Sync case: `UseAwait_Async(False)`

In this case, `DoJob` would use `.Result`.

The output:
```text
EventRecord { Id = 0, Status = Running, TimeMs = 0 }
EventRecord { Id = 0, Status = RanToCompletion, TimeMs = 300 }
EventRecord { Id = 1, Status = Running, TimeMs = 300 }
EventRecord { Id = 1, Status = RanToCompletion, TimeMs = 601 }
EventRecord { Id = 2, Status = Running, TimeMs = 601 }
EventRecord { Id = 2, Status = RanToCompletion, TimeMs = 902 }
EventRecord { Id = 3, Status = Running, TimeMs = 902 }
EventRecord { Id = 3, Status = RanToCompletion, TimeMs = 1203 }
EventRecord { Id = 4, Status = Running, TimeMs = 1203 }
EventRecord { Id = 4, Status = RanToCompletion, TimeMs = 1503 }
EventRecord { Id = 5, Status = Running, TimeMs = 1503 }
EventRecord { Id = 5, Status = RanToCompletion, TimeMs = 1804 }
EventRecord { Id = 6, Status = Running, TimeMs = 1804 }
EventRecord { Id = 6, Status = RanToCompletion, TimeMs = 2104 }
EventRecord { Id = 7, Status = Running, TimeMs = 2104 }
EventRecord { Id = 7, Status = RanToCompletion, TimeMs = 2405 }
EventRecord { Id = 8, Status = Running, TimeMs = 2405 }
EventRecord { Id = 8, Status = RanToCompletion, TimeMs = 2706 }
EventRecord { Id = 9, Status = Running, TimeMs = 2706 }
EventRecord { Id = 9, Status = RanToCompletion, TimeMs = 3006 }
EventRecord { Id = 2147483647, Status = RanToCompletion, TimeMs = 3006 }
```

As observed in the output, all the `DoJob` tasks were executed one by one.
Without `await`, `DoJob` is not able to utilize the `async` feature.

In the end, we still completed 10 tasks, each took about 300ms. But since they are executed one-by-one, we took a total of about 300*10ms.
