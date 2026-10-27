// See https://aka.ms/new-console-template for more information

var embeddingService = new OllamaEmbeddingService();
var semanticSearch = new SemanticSearch(embeddingService);
var chatService = new OllamaChatService(new HttpClient { Timeout = TimeSpan.FromMinutes(2) });


 //Console.WriteLine(ducument);
var chunks = await new Chunking().ChunkingAsync();

// Index documents once
 await semanticSearch.IndexAsync(chunks);

while (true)
{
    Console.Write("\nQuestion (empty = exit): ");
    var question = Console.ReadLine();
    if (string.IsNullOrWhiteSpace(question)) break;

    var relevantChunks = await semanticSearch.SearchAsync(question, 0.3, 2);
    if (relevantChunks.Count == 0)
    {
        Console.WriteLine("No relevant information found.");
        continue;
    }

    Console.WriteLine(await chatService.GenerateAnswerAsync(question, relevantChunks));
}
