Handling Exceptions from `Task.WhenAll`
====================================

Key points to takeaway:
- `Task.WhenAll` is a `Task`, which completes when all its sub-tasks are completed.
- An exception thrown by a sub-task does not interrupt WhenAll task nor the other sub-tasks.
- When there are exception(s) thrown by sub-tasks
  - The WhenAll task `Exception` property would be `AggregationException`
  - The sub-tasks exceptions will be placed into the above `AggregationException`'s `InnerExceptions` collection property
  - a `try` & `catch` `Exception` on `await WhenAll` will catch the `AggregationException`'s `InnerException` property, which is the same as `InnerExceptions`'s first element.

Please check [WhenAllTests](WhenAllTests.cs) for demonstration.
