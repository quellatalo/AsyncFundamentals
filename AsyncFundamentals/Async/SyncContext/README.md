The behaviors of `ConfigureAwait` based on the boolean `continueOnCapturedContext`
==================================================================================


# Does it matter?

Yes, and no.

If we do not care about the performance of our code, the default behavior will most likely bring us to the expected result.

_Note: when we don't specify `ConfigureAwait`, it means we're using `ConfigureAwait(true)`.
Yes, `continueOnCapturedContext` is `true` by default._

But when we do care about performance and the stability of our software, this is important.

References:
- In `ConfigureAwait`'s [documentation](https://learn.microsoft.com/en-us/dotnet/api/System.Threading.Tasks.Task.ConfigureAwait?view=net-8.0#system-threading-tasks-task-configureawait(system-boolean)), Microsoft does insist the devs to read [an article](https://devblogs.microsoft.com/dotnet/configureawait-faq/) (mentioned in both **Remarks** and **See also**).
- Interesting [PR](https://github.com/dotnet/corefx/pull/38610/files) on .NET's `System.Net.Http`

# Overview

The [FAQ article](https://devblogs.microsoft.com/dotnet/configureawait-faq/) may explain it better.
Still, let's try to simplify it.

About the parameter `continueOnCapturedContext`, [the documentation](https://learn.microsoft.com/en-us/dotnet/api/System.Threading.Tasks.Task.ConfigureAwait?view=net-8.0#system-threading-tasks-task-configureawait(system-boolean)) states:
> `true` to attempt to marshal the continuation back to the original context captured; otherwise, `false`.

The "context" here means `System.Diagnostics.SynchronizationContext`, which is quite common in applications that strongly control multi-threading behaviors.
For example, desktop applications like WinForm or Wpf have a SynchronizationContext to manage multi-threading interactions and maintain consistent UI behaviors.

We shall prepare a mock UI-like SynchronizationContext ([UiContext.cs](UiContext.cs))

There are 2 test files:
- [ConfigureAwaitTests](ConfigureAwaitTests.cs): Demonstrate how `continueOnCapturedContext` work with `SynchronizationContext`
- [UiUpdateTests](UiUpdateTests.cs): Reproduce the behaviors when working with UI
  - When we should use `ConfigureAwait(true)`
  - When we should use `ConfigureAwait(false`

Most of the output in our tests will have the current thread ID as prefix for us to observe the "marshaling" mentioned in the documentation.

# Setup Fixture

[Fixture.cs](Fixture.cs) is where we prepare a [UiContext](UiContext.cs) to mock the `SynchronizationContext` used by UI applications.

UiContext's details:
- There's a (mock) UI thread constantly running, which prints a heartbeat every 500ms
- UiContent considers the UI thread as the main thread, and queue all executions synchronously to it

Also, the fixture will also create a `Fixture.DefaultContext` from the default class `SynchronizationContext`.
Its behavior would be no different from having no `SynchronizationContext` set.

From the beginning, the fixture would have the following output:
```text
14: Synchronization context:
14: Created context: UiContext created under thread 14, with underlying UI thread 15
15: UI heartbeat after 0 ms
15: UI heartbeat after 508 ms
15: UI heartbeat after 502 ms
...
```

Let's keep track of the **UI thread 15** within this document.

# [ConfigureAwaitTests](ConfigureAwaitTests.cs)

To make it simple, the test methods are plain `void` methods, without `async` keyword.
So, it does not do anything async on by itself. Everything is linear to observe.

The general test scenario for all test methods:
- Set `SynchronizationContext` to the desired scenario
- Execute [AwaitTaskThatPrintsRunningThread](ConfigureAwaitTests.cs#L61) method
- Wait for executions to finish and assert the situation

The `async` method [AwaitTaskThatPrintsRunningThread](ConfigureAwaitTests.cs#L61) would:
- `await` execute some task (just print something to console) `ConfigureAwait`
  - the parameter `continueOnCapturedContext` would be used here
- do something that may take time
  - in "Fast" scenario, it takes no time at all
  - in "Slow" scenario, it takes 1000ms

The "Fast" scenario would be sufficient to showcase the behaviors.
The "Slow" scenario is to demonstrate how a misusage can affect performance.

## DefaultContext test

It the test method `DefaultContext_Fast_UnableToContinueCapturedContext`.

There are totally 4 test cases, but they behavior similar to each other:

| continueOnCapturedContext | No context set               | Fixture.DefaultContext       |
|---------------------------|------------------------------|------------------------------|
| true                      | unpredictable thread numbers | unpredictable thread numbers |
| false                     | unpredictable thread numbers | unpredictable thread numbers |

Output:
```text
0: Synchronization context: System.Threading.SynchronizationContext
14: Awaiting task (True)
7: Task executing
7: Done await(True)
```

We can see that the async feature is utilized and the works are being done in different available threads.

_Note: It is expected that the **UI thread 15** should not be utilized in this test._

## SyncContext test

It is the test method `SyncContext_Fast_ContinueOnCapturedContext`.

In this one, we set the SynchronizationContext to the [Fixture](Fixture.cs)'s prepared [UiContext](UiContext.cs)

There are only 2 cases:
- `continueOnCapturedContext` is `false`
- `continueOnCapturedContext` is `true`

### SyncContext without `continueOnCapturedContext`

In this case, `continueOnCapturedContext` is `false`.

The output:
```text
0: Synchronization context set: UiContext created under thread 14, with underlying UI thread 15
14: Awaiting task (False)
12: Task executing
12: Done await(False)
```

It still looks just like the previous test result: `unpredictable thread numbers`.
The works are being done in different available threads.

### SyncContext with `continueOnCapturedContext`

In this case, `continueOnCapturedContext` is `true`.

The output:
```text
12: Synchronization context set: UiContext created under thread 14, with underlying UI thread 15
14: Awaiting task (True)
7: Task executing
15: Done await(True)
```

Notice how after "Task executing", which is the line of `await` with `ConfigureAwait(true)`, the executing thread is set to **15** - the UI thread.

As `continueOnCaptureContext` means "to attempt to marshal the continuation back to the original context captured", and the scenario is showing this behavior.

We did not see in other cases because either `continueOnCaptureContext` is false, or the `SynchronizationContext` does not have such control.

## SyncContext Slow test

This test `SyncContext_Slow_ContinueOnCapturedContext` is just similar to [SyncContext test](#synccontext-test).
The only difference is that after "Task executing" then "Done await", we would execute something that take time.
Particularly, we sleep the thread for 1000ms.

Also, we print the heartbeat history to observe.

### SyncContext Slow without `continueOnCapturedContext`

It behaves just like [the Fast counterpart](#synccontext-without-continueoncapturedcontext).
The only difference is that this takes longer to finish.

The output:
```text
0: Synchronization context set: UiContext created under thread 14, with underlying UI thread 15
14: Awaiting task (False)
12: Task executing
12: Done await(False)
[0,505,513,508]
```

Notice that the heartbeat array at the end is stable: each beat occurs about 500ms away from the last.

### SyncContext Slow with `continueOnCapturedContext`

Just like [the Fast counterpart](#synccontext-with-continueoncapturedcontext), this case will marshal back to use **the UI thread 15** at the end.
Therefore, the 1000ms time-consuming job would affect **the UI thread 15**.

The output:
```text
0: Synchronization context set: UiContext created under thread 14, with underlying UI thread 15
14: Awaiting task (True)
5: Task executing
15: Done await(True)
[0,500,1528]
```

Notice the heartbeat is stalled.
There is one that triggered 1000ms higher than the rest.

_An actual implementation of this case on a real UI context is provided in the [DemoWpfApp](../../../DemoWpfApp) project.
We can try running the app and click the "BackEnd await true: UI hang" button.
The UI would hang for some time (just like the long heartbeat)._

# [UiUpdateTests](UiUpdateTests.cs)

Up until now, we see that using `ConfigureAwait(false)` would generally utilize asynchronous operations better than the default `ConfigureAwait(true)`.
However, sometimes we do need "to marshal the continuation back to the original context" for a good reason.

Let's take UI applications for example.
The UI represents the values of various objects.
For such objects, the UI thread would want to own them to consistently manage UI changes and prevent unpredictable errors.

In this example, [UiContext](UiContext.cs) has prepared an object `UiContent` that is exclusively reserved for **the UI thread**.

To make it simple, the test methods are plain `void` methods, without `async` keyword.
So, it does not do anything async on by itself. Everything is linear to observe.

The general test scenario for all test methods:
- Set `SynchronizationContext` to `Fixture.UiContext`
- Execute [AwaitTaskUpdateUi](UiUpdateTests.cs#L39) method
- Wait for executions to finish and assert the situation

The `async` method [AwaitTaskUpdateUi](UiUpdateTests.cs#L39) would:
- `await` some string with `ConfigureAwait`
  - the parameter `continueOnCapturedContext` would be used here
- update `UiContent` to the value awaited from the previous step

## Update UI Content without `continueOnCapturedContext`

Similar to the `continueOnCapturedContext` `false` of `SyncContext_Fast_ContinueOnCapturedContext` and `SyncContext_Slow_ContinueOnCapturedContext`, the update is executed a thread other than **the UI thread 15**.
However, since `UiContent` is reserved for **the UI thread 15**, an exception was thrown.

_An actual implementation of this case on a real UI context is provided in the [DemoWpfApp](../../../DemoWpfApp) project.
We can try running the app and click the "UI await false: InvalidOperation" button._

## Update UI Content with `continueOnCapturedContext`

The test method is `UpdateUiContent_ContinueOnCapturedContext_Updated`.

Similar to the `continueOnCapturedContext` `true` of `SyncContext_Fast_ContinueOnCapturedContext` and `SyncContext_Slow_ContinueOnCapturedContext`, the update is executed on **the UI thread 15**, and it is successful.
