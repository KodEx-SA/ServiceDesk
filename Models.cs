using Microsoft.EntityFrameworkCore;

public class ServiceRequest
{
    public int Id { get; set; }
    public string Reference { get; set; } = "";
    public string Description { get; set; } = "";
    public string? Location { get; set; }
    public string Category { get; set; } = "";
    public string Department { get; set; } = "";
    public string Urgency { get; set; } = "";
    public string Summary { get; set; } = "";
    public string ClassifiedBy { get; set; } = "";
    public string Status { get; set; } = "received";
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class ServiceDeskDb : DbContext
{
    public ServiceDeskDb(DbContextOptions<ServiceDeskDb> options) : base(options) { }
    public DbSet<ServiceRequest> Requests => Set<ServiceRequest>();
}