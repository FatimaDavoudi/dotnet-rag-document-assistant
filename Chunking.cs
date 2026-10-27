
using System.Text.RegularExpressions;

public class Chunking
{
	public async Task<List<string>> ChunkingAsync()
	{
		string baseDirectory = AppContext.BaseDirectory;
		string filePath = Path.Combine(
			baseDirectory,
			"Documents",
			"support-guide.txt");

		try
		{
			string document = await File.ReadAllTextAsync(filePath);
            if (string.IsNullOrWhiteSpace(document))
            {
                Console.WriteLine("Support guide is empty.");
                return new List<string>();
            }
			
			var chunks = SplitIntoChunks(document);
			return chunks;
		}
		catch (Exception ex)
		{
			Console.WriteLine($"Error reading support guide: {ex.Message}");
			return new List<string>();
		}
	}

    List<string> SplitIntoChunks(string document)
    {
		 // 1. In Absätze splitten (Leerzeile = Trenner, funktioniert mit \n und \r\n)
    var paragraphs = Regex.Split(document.Trim(), @"(?:\r?\n\s*){2,}")
        .Select(p => p.Trim())
        .Where(p => p.Length > 0)
        .ToList();

    // 2. Überschrift = einzeilig, kurz, kein Satzzeichen am Ende (oder beginnt mit '#')
    static bool IsHeading(string p) =>
        p.StartsWith('#') ||
        (!p.Contains('\n') && p.Length <= 80 && !p.EndsWith('.') && !p.EndsWith('?') && !p.EndsWith('!'));

    // 3. Titel = erster Absatz (falls Überschrift), wird jedem Chunk vorangestellt
    string title = "";
    int start = 0;
    if (paragraphs.Count > 0 && IsHeading(paragraphs[0]))
    {
        title = paragraphs[0].TrimStart('#', ' ');
        start = 1;
    }

    // 4. Überschrift + zugehörige Absätze zu einem Chunk zusammenfassen
    var chunks = new List<string>();
    var current = new List<string>();

    void Flush()
    {
        if (current.Count == 0) return;
        var body = string.Join("\n\n", current);
        chunks.Add(title.Length > 0 ? $"{title}\n\n{body}" : body);
        current.Clear();
    }

    for (int i = start; i < paragraphs.Count; i++)
    {
        if (IsHeading(paragraphs[i]))
            Flush();                       // neue Überschrift = neuer Chunk
        current.Add(paragraphs[i].TrimStart('#', ' '));
    }
    Flush();

    return chunks;
    }
}