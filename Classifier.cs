public record Classification(string Category, string Department, string Urgency, string Summary, string ClassifiedBy);

public interface IClassifier
{
    Task<Classification> ClassifyAsync(string text);
}

public class RuleClassifier : IClassifier
{
    private static readonly Dictionary<string, string[]> Keywords = new()
    {
        ["water"] = ["water", "pipe", "leak", "sewage", "sewer", "tap", "drain", "toilet"],
        ["roads"] = ["pothole", "road", "street", "traffic", "robot", "streetlight", "bridge"],
        ["electricity"] = ["electric", "power", "outage", "transformer", "cable", "wire", "load shedding"],
        ["waste"] = ["refuse", "rubbish", "garbage", "waste", "dumping", "bin"],
        ["housing_documents"] = ["house", "rdp", "permit", "certificate", "id document", "application", "title deed"],
    };

    private static readonly Dictionary<string, string> Departments = new()
    {
        ["water"] = "Water & Sanitation",
        ["roads"] = "Roads & Transport",
        ["electricity"] = "Electricity & Energy",
        ["waste"] = "Waste Management",
        ["housing_documents"] = "Human Settlements & Documents",
        ["other"] = "General Enquiries",
    };

    private static readonly string[] High = ["burst", "no water", "exposed wire", "sewage overflow", "fire", "emergency", "gas", "danger", "flood", "sparking", "injur"];
    private static readonly string[] Medium = ["days", "week", "again", "blocked", "broken", "overflow", "no power"];

    public Task<Classification> ClassifyAsync(string text)
    {
        var t = text.ToLowerInvariant();
        var best = "other";
        var bestScore = 0;
        foreach (var (category, words) in Keywords)
        {
            var score = words.Count(w => t.Contains(w));
            if (score > bestScore) { best = category; bestScore = score; }
        }

        var urgency = High.Any(w => t.Contains(w)) ? "high"
                    : Medium.Any(w => t.Contains(w)) ? "medium" : "low";

        var trimmed = text.Trim();
        var cut = trimmed.IndexOfAny(new[] { '.', '!', '?' });
        var first = cut > 0 ? trimmed[..(cut + 1)] : trimmed;
        var summary = first.Length > 120 ? first[..117] + "..." : first;

        return Task.FromResult(new Classification(best, Departments[best], urgency, summary, "rules"));
    }
}