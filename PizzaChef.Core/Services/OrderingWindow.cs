namespace PizzaChef.Core.Services;

public sealed record OrderingWindow(DateOnly Day, bool IsOpen, DateTimeOffset CutoffLocal, DateTimeOffset NowLocal);
