
public static class RagEvaluationReporter
{
	public static void PrintResults(
		IReadOnlyList<RagEvaluation.EvaluationResult> results)
	{
		  ArgumentNullException.ThrowIfNull(results);

    var passed = results.Count(result => result.Passed);
    var failed = results.Count - passed;

    for (int i = 0; i < results.Count; i++)
    {
        var result = results[i];

        Console.WriteLine();
        Console.WriteLine($"===== Test Case {i + 1} =====");
        Console.WriteLine($"Status: {(result.Passed ? "PASSED" : "FAILED")}");

        Console.WriteLine("\nRetrieved Chunks:");

        foreach (var chunk in result.RetrievedChunks)
        {
            Console.WriteLine($"- {chunk}");
        }

        Console.WriteLine("\nLLM Answer:");
        Console.WriteLine(result.Answer);

        Console.WriteLine("\nErrors:");

        if (result.Errors.Count == 0)
        {
            Console.WriteLine("None");
        }
        else
        {
            foreach (var error in result.Errors)
            {
                Console.WriteLine($"- {error}");
            }
        }
    }

    Console.WriteLine("\n===== Evaluation Summary =====");
    Console.WriteLine($"Total Tests: {results.Count}");
    Console.WriteLine($"Passed: {passed}");
    Console.WriteLine($"Failed: {failed}");

    var successRate = results.Count == 0
        ? 0
        : (double)passed / results.Count * 100;

    Console.WriteLine($"Success Rate: {successRate:F2}%");
	}
}
