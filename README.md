C# Async Fundamentals
=====================

This repository demonstrates the usage of `await` and `ConfigureAwait(true)`/`ConfigureAwait(false)`.

# Overview

The main topics:
- [Await](AsyncFundamentals/Async/Await): The difference between `await` and `.Wait()`/`.Result`
- [WhenAll's Exception](AsyncFundamentals/Async): Exception handling with `Task.WhenAll`
- [ContinueOnCapturedContext](AsyncFundamentals/Async/SyncContext): `ConfigureAwait` usages
  - when to use `ConfigureAwait(false)`
  - When to use `ConfigureAwait(true)`

There's also the Wpf Application [DemoWpfApp](DemoWpfApp) to experience some cases described about

# Further reading

For more details, maybe this article can help: https://devblogs.microsoft.com/dotnet/configureawait-faq/
