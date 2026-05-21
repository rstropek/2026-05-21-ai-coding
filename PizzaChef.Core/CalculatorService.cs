namespace PizzaChef.Core;

public interface ICalculatorService
{
    int Add(int left, int right);
}

public sealed class CalculatorService : ICalculatorService
{
    public int Add(int left, int right) => left + right;
}
