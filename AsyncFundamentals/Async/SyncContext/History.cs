using System.Collections;

namespace AsyncFundamentals.Async.SyncContext;

public class History<T>(int maxSize) : IEnumerable<T>
{
    readonly LinkedList<T> _linkedList = [];

    public int MaxSize { get; } = maxSize;

    public void Add(T item)
    {
        if (_linkedList.Count == MaxSize)
        {
            _linkedList.RemoveFirst();
        }

        _linkedList.AddLast(item);
    }

    public void Clear() => _linkedList.Clear();

    /// <inheritdoc/>
    public IEnumerator<T> GetEnumerator() => _linkedList.GetEnumerator();

    /// <inheritdoc/>
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
