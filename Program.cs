var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSingleton<IClassifier, RuleClassifier>();   // register
var app = builder.Build();

app.MapGet("/", () => "Hello NW Service Desk!");

app.MapPost("/api/requests", async (CreateRequest body, IClassifier classifier) =>   // injected
{
    var description = body.Description?.Trim() ?? "";
    if (description.Length is < 10 or > 2000)
        return Results.BadRequest(new { error = "description must be 10-2000 characters" });

    var c = await classifier.ClassifyAsync(description);
    var reference = "NW" + Guid.NewGuid().ToString("N")[..8].ToUpper();
    return Results.Created($"/api/requests/{reference}",
        new { reference, c.Category, c.Department, c.Urgency, c.Summary });
});

app.Run();

record CreateRequest(string? Description, string? Location);