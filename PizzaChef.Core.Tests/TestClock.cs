using PizzaChef.Core.Time;

namespace PizzaChef.Core.Tests;

internal sealed class TestClock : IClock
{
    public TestClock(DateTimeOffset utcNow)
    {
        UtcNow = utcNow;
    }

    public DateTimeOffset UtcNow { get; set; }
}
