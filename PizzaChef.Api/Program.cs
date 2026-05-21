using PizzaChef.Core;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddSingleton<ICalculatorService, CalculatorService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.MapGet("/api/calculator/add", (int left, int right, ICalculatorService calculator) =>
{
    var result = calculator.Add(left, right);

    return TypedResults.Ok(new AddResponse(result));
})
.WithName("Add")
.WithSummary("Adds two integers.")
.WithDescription("Returns the sum of the left and right query parameters.")
.WithTags("Calculator");

app.MapGet("/", () => Results.Redirect("/openapi/v1.json"))
    .ExcludeFromDescription();

app.Run();

public sealed record AddResponse(int Result);
