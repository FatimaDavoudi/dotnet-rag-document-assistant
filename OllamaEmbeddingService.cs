using System.Net.Http.Json;
using System.Text.Json.Serialization;

public class OllamaEmbeddingService
{
    private class EmbeddingRequest
    {
        [JsonPropertyName("model")]
        public string Model { get; set; } = "";

        [JsonPropertyName("input")]
        public string Input { get; set; } = "";
    }

    private class EmbeddingResponse
    {
        [JsonPropertyName("embeddings")]
        public List<float[]> Embeddings { get; set; } = new List<float[]>();
    }

     private readonly HttpClient _httpClient;

    public OllamaEmbeddingService()
    {
        _httpClient = new HttpClient
        {
            BaseAddress = new Uri("http://localhost:11434")
        };
    }

    public async Task<float[]> GenerateEmbeddingAsync(string text)
    {
        var request = new EmbeddingRequest
        {
            Model = "all-minilm:latest",
            Input = text
        };

        var response = await _httpClient.PostAsJsonAsync(
            "/api/embed",
            request);

        response.EnsureSuccessStatusCode();

        var result = await response.Content
            .ReadFromJsonAsync<EmbeddingResponse>();

        return result?.Embeddings[0]
            ?? throw new InvalidOperationException(
                "Ollama returned no embedding.");
    }
}