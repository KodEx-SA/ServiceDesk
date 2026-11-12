using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSingleton<IClassifier, RuleClassifier>();
builder.Services.AddDbContext<ServiceDeskDb>(o => o.UseSqlite("Data Source=servicedesk.db"));
var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    scope.ServiceProvider.GetRequiredService<ServiceDeskDb>().Database.EnsureCreated();
}

app.MapGet("/", () => "Hello NW Service Desk!");

app.MapPost("/api/requests", async (CreateRequest body, IClassifier classifier, ServiceDeskDb db) =>
{
    var description = body.Description?.Trim() ?? "";
    if (description.Length is < 10 or > 2000)
        return Results.BadRequest(new { error = "description must be 10-2000 characters" });

    var c = await classifier.ClassifyAsync(description);
    var reference = "NW" + Guid.NewGuid().ToString("N")[..8].ToUpper();
    var now = DateTime.UtcNow;

    db.Requests.Add(new ServiceRequest
    {
        Reference = reference,
        Description = description,
        Location = body.Location?.Trim(),
        Category = c.Category,
        Department = c.Department,
        Urgency = c.Urgency,
        Summary = c.Summary,
        ClassifiedBy = c.ClassifiedBy,
        CreatedAt = now,
        UpdatedAt = now,
    });
    await db.SaveChangesAsync();

    return Results.Created($"/api/requests/{reference}",
        new { reference, c.Category, c.Department, c.Urgency, c.Summary });
});

app.MapGet("/api/requests/{reference}", async (string reference, ServiceDeskDb db) =>
{
    var r = await db.Requests
        .Where(x => x.Reference == reference)
        .Select(x => new { x.Reference, x.Category, x.Department, x.Status, x.CreatedAt, x.UpdatedAt })
        .FirstOrDefaultAsync();
    return r is null ? Results.NotFound(new { error = "not found" }) : Results.Ok(r);
});

app.Run();

record CreateRequest(string? Description, string? Location);