using System.Diagnostics;
using System.Text;

namespace AsyncFundamentals.Async.Await;

public record EventRecord(int Id, TaskStatus Status = TaskStatus.Running) : IComparable<EventRecord>
{
    static readonly Stopwatch s_processWatch = Stopwatch.StartNew();

    public long TimeMs { get; } = s_processWatch.ElapsedMilliseconds;

    public static bool operator <(EventRecord left, EventRecord right)
        => ReferenceEquals(left, null) ? !ReferenceEquals(right, null) : left.CompareTo(right) < 0;

    public static bool operator <=(EventRecord left, EventRecord right)
        => ReferenceEquals(left, null) || left.CompareTo(right) <= 0;

    public static bool operator >(EventRecord left, EventRecord right)
        => !ReferenceEquals(left, null) && left.CompareTo(right) > 0;

    public static bool operator >=(EventRecord left, EventRecord right)
        => ReferenceEquals(left, null) ? ReferenceEquals(right, null) : left.CompareTo(right) >= 0;

    public int CompareTo(EventRecord? other)
    {
        if (ReferenceEquals(this, other))
        {
            return 0;
        }

        if (other is null)
        {
            return 1;
        }

        int timeComparison = Id.CompareTo(other.Id);
        if (timeComparison != 0)
        {
            return timeComparison;
        }

        int kindComparison = Status.CompareTo(other.Status);
        if (kindComparison != 0)
        {
            return kindComparison;
        }

        return TimeMs.CompareTo(other.TimeMs);
    }

    public static string EventsToString(IEnumerable<EventRecord> value)
    {
        ArgumentNullException.ThrowIfNull(value);
        StringBuilder sb = new();
        foreach (var record in value)
        {
            sb.AppendLine(record.ToString());
        }

        return sb.ToString();
    }

    public static void RestartWatch() => s_processWatch.Restart();
}
