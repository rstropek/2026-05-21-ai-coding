using PizzaChef.Core;

namespace PizzaChef.Core.Tests;

public class CalculatorServiceTests
{
    [Fact]
    public void Add_ReturnsSum()
    {
        var calculator = new CalculatorService();

        var result = calculator.Add(1, 2);

        Assert.Equal(3, result);
    }

    [Theory]
    [InlineData(0, 0, 0)]
    [InlineData(-2, 5, 3)]
    [InlineData(-4, -6, -10)]
    public void Add_HandlesCommonIntegerValues(int left, int right, int expected)
    {
        var calculator = new CalculatorService();

        var result = calculator.Add(left, right);

        Assert.Equal(expected, result);
    }
}
