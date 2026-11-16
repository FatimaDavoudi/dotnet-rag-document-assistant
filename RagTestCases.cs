public class RagTestCases
{
public static IReadOnlyList<RagTestCase> GetAll() => TestCases;

    
public static IReadOnlyList<RagTestCase> TestCases { get; } = new List<RagTestCase>
{
    new()
    {
        Question = "What should be checked when a database connection timeout occurs?",
        ExpectedKeywords = new List<string>
        {
            "database server status",
            "network connectivity",
            "firewall rules",
            "connection string"
        },
        ShouldBeAnswerable = true
    },

    new()
    {
        Question = "What should be checked when authentication fails?",
        ExpectedKeywords = new List<string>
        {
            "username",
            "password",
            "authentication configuration"
        },
        ShouldBeAnswerable = true
    },

    new()
    {
        Question = "How can I reset a forgotten database password?",
        ExpectedKeywords = new List<string>(),
        ShouldBeAnswerable = false
    }
}.AsReadOnly();
}
