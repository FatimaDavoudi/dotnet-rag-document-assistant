public class SemanticSearch
{
    private readonly OllamaEmbeddingService _embeddingService;
    private readonly List<(string Chunk, float[] Embedding)> _documents = new();

    public SemanticSearch(OllamaEmbeddingService embeddingService)
    {
        _embeddingService = embeddingService;
    }

    public async Task IndexAsync(List<string> chunks)
    {
        _documents.Clear();

        foreach (string chunk in chunks)
        {
            float[] embedding =
                await _embeddingService.GenerateEmbeddingAsync(chunk);

            _documents.Add((chunk, embedding));
        }

        Console.WriteLine($"Indexed {_documents.Count} chunks.");
    }

    public async Task<List<string>> SearchAsync(string question, double relevanceThreshold = 0.5, int topK = 2)
    {
        if (string.IsNullOrWhiteSpace(question))
				throw new ArgumentException("A question is required.", nameof(question));

        if (relevanceThreshold < -1 || relevanceThreshold > 1)
				throw new ArgumentException("Relevance threshold must be between -1 and 1.", nameof(relevanceThreshold));     

         if (topK <= 0)
				throw new ArgumentException("TopK must be greater than 0.", nameof(topK));                
        float[] questionEmbedding =
            await _embeddingService.GenerateEmbeddingAsync(question);

  
        var results = _documents
            .Select(document => (
                document.Chunk,
                Score: CosineSimilarity(
                    questionEmbedding,
                    document.Embedding)))
                    .Where(result => result.Score >= relevanceThreshold)
            .OrderByDescending(result => result.Score)
            .Take(topK)
            .ToList();
        if (results.Count == 0)
        {
            return new List<string>();
        }
        return results.Select(r => r.Chunk).ToList();
    }


    private static double CosineSimilarity(float[] a, float[] b)
    {
        double dotProduct = 0;
        double normA = 0;
        double normB = 0;

        for (int i = 0; i < a.Length; i++)
        {
            dotProduct += a[i] * b[i];
            normA += a[i] * a[i];
            normB += b[i] * b[i];
        }

        if (normA == 0 || normB == 0)
            return 0;

        return dotProduct / (Math.Sqrt(normA) * Math.Sqrt(normB));
    }

    public async Task<List<(string Chunk, double Score)>> SearchWithScoresAsync(string question, int topK = 3)
{
    var q = await _embeddingService.GenerateEmbeddingAsync(question);
    Console.WriteLine($"Frage-Vektor: {q.Length} Dimensionen");

    return _documents
        .Select(d => (d.Chunk, Score: CosineSimilarity(q, d.Embedding)))
        .OrderByDescending(r => r.Score)
        .Take(topK)
        .ToList();
}
}