// See https://aka.ms/new-console-template for more information

var embeddingService = new OllamaEmbeddingService();
var semanticSearch = new SemanticSearch(embeddingService);
var chatService = new OllamaChatService(new HttpClient { Timeout = TimeSpan.FromMinutes(2) });


 //Console.WriteLine(ducument);
var chunks = await new Chunking().ChunkingAsync();

// Index documents once
try
{
    await semanticSearch.IndexAsync(chunks);
}
catch (HttpRequestException ex)
{
    Console.WriteLine("Error: Could not connect to Ollama.");
    Console.WriteLine($"Details: {ex.Message}");
    Console.WriteLine("Please make sure Ollama is running.");
    return;
}

// Initialize RAG evaluation
var evaluation = new RagEvaluation(
    semanticSearch,
    chunk => chunk.ToString() ?? string.Empty,
    (question, chunks) => chatService.GenerateAnswerAsync(
        question,
        chunks.ToList()));

// User interaction
Console.WriteLine("RAG Document Assistant");
Console.WriteLine("Type 'test' to run evaluation.");
Console.WriteLine("Type 'exit' to quit.");

while (true)
{
    Console.Write("\nYour question: ");
    var input = Console.ReadLine();

    if (string.IsNullOrWhiteSpace(input))
        continue;

    if (input.Equals("exit", StringComparison.OrdinalIgnoreCase))
        break;

try
{
    if (input.Equals("test", StringComparison.OrdinalIgnoreCase))
    {
        Console.WriteLine("\nStarting RAG Evaluation...\n");

        var results = await evaluation.EvaluateAllAsync();

        RagEvaluationReporter.PrintResults(results);

        continue;
    }

    // Normal RAG question
    var relevantChunks = await semanticSearch.SearchAsync(input, 0.3, 2);
   
    if (relevantChunks.Count == 0)
    {
        Console.WriteLine(
            "The information is not available in the provided context.");
        continue;
    }

    var answer = await chatService.GenerateAnswerAsync(
        input,
        relevantChunks);

    Console.WriteLine($"\nAnswer: {answer}");
}
catch (HttpRequestException ex)
{
    Console.WriteLine("Error: Could not connect to Ollama.");
    Console.WriteLine($"Details: {ex.Message}");
    Console.WriteLine("Please make sure Ollama is running.");
}

Console.WriteLine("Goodbye!");
}
// while (true)
// {
//     Console.Write("\nQuestion (empty = exit): ");
//     var question = Console.ReadLine();
//     if (string.IsNullOrWhiteSpace(question)) break;

//     var relevantChunks = await semanticSearch.SearchAsync(question, 0.3, 2);
//     if (relevantChunks.Count == 0)
//     {
//         Console.WriteLine("No relevant information found.");
//         continue;
//     }
//     Console.WriteLine(await chatService.GenerateAnswerAsync(question, relevantChunks));
// }
