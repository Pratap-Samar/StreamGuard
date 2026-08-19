namespace StreamGuard;

public sealed record FrequencyCount(string Value, long Count);

public sealed class BoundedFrequencyCounter
{
    public const int DefaultCapacity = 100;

    private readonly int capacity;
    private readonly Dictionary<string, long> counts = new(StringComparer.Ordinal);

    public BoundedFrequencyCounter(int capacity = DefaultCapacity)
    {
        if (capacity <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(capacity));
        }

        this.capacity = capacity;
    }

    public int Count => counts.Count;

    public void Observe(string value)
    {
        if (counts.TryGetValue(value, out long count))
        {
            counts[value] = count + 1;
            return;
        }

        if (counts.Count < capacity)
        {
            counts[value] = 1;
            return;
        }

        string leastFrequentValue = counts.MinBy(pair => pair.Value).Key;
        long leastFrequentCount = counts[leastFrequentValue];

        counts.Remove(leastFrequentValue);
        counts[value] = leastFrequentCount + 1;
    }

    public IReadOnlyList<FrequencyCount> GetTop(int maxItems = int.MaxValue)
    {
        if (maxItems < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(maxItems));
        }

        return counts
            .OrderByDescending(pair => pair.Value)
            .ThenBy(pair => pair.Key, StringComparer.Ordinal)
            .Take(maxItems)
            .Select(pair => new FrequencyCount(pair.Key, pair.Value))
            .ToArray();
    }
}
