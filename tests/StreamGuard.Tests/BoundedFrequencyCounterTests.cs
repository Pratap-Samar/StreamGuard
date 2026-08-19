using StreamGuard;

namespace StreamGuard.Tests;

public class BoundedFrequencyCounterTests
{
    [Fact]
    public void Counter_DefaultCapacityIsOneHundred()
    {
        BoundedFrequencyCounter counter = new();

        for (int index = 0; index <= BoundedFrequencyCounter.DefaultCapacity; index++)
        {
            counter.Observe($"value-{index}");
        }

        Assert.Equal(100, counter.Count);
    }

    [Fact]
    public void Counter_DoesNotExceedCapacity()
    {
        BoundedFrequencyCounter counter = new(2);

        counter.Observe("alice");
        counter.Observe("bob");
        counter.Observe("carol");

        Assert.Equal(2, counter.Count);
    }

    [Fact]
    public void Counter_PrioritizesHigherFrequencyValues()
    {
        BoundedFrequencyCounter counter = new(2);

        counter.Observe("alice");
        counter.Observe("alice");
        counter.Observe("alice");
        counter.Observe("bob");
        counter.Observe("carol");
        counter.Observe("carol");
        counter.Observe("carol");

        IReadOnlyList<FrequencyCount> top = counter.GetTop();

        Assert.Equal("carol", top[0].Value);
        Assert.True(top[0].Count > top[1].Count);
    }

    [Fact]
    public void Counter_IncrementsExistingValuesAndOrdersByFrequency()
    {
        BoundedFrequencyCounter counter = new(3);

        counter.Observe("alice");
        counter.Observe("bob");
        counter.Observe("alice");

        IReadOnlyList<FrequencyCount> top = counter.GetTop();

        Assert.Equal(2, top.Count);
        Assert.Equal("alice", top[0].Value);
        Assert.Equal(2, top[0].Count);
        Assert.Equal("bob", top[1].Value);
        Assert.Equal(1, top[1].Count);
    }
}
