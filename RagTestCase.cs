public sealed class RagTestCase
{
    public string Question { get; set; } = string.Empty;

    public List<string> ExpectedKeywords { get; set; } = new();

    public bool ShouldBeAnswerable { get; set; }
}