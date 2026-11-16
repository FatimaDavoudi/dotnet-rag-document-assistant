using System.Collections;


public sealed class RagEvaluation
{
	private const string UnavailableAnswer = "the information is not available";
	private readonly dynamic semanticSearch;
	private readonly Func<object, string> getChunkText;
	private readonly Func<string, IReadOnlyList<string>, Task<string>> generateAnswerAsync;

	public RagEvaluation(
		object semanticSearch,
		Func<object, string> getChunkText,
		Func<string, IReadOnlyList<string>, Task<string>> generateAnswerAsync)
	{
		this.semanticSearch = semanticSearch ?? throw new ArgumentNullException(nameof(semanticSearch));
		this.getChunkText = getChunkText ?? throw new ArgumentNullException(nameof(getChunkText));
		this.generateAnswerAsync = generateAnswerAsync ?? throw new ArgumentNullException(nameof(generateAnswerAsync));
	}

	public async Task<EvaluationResult> EvaluateAsync(RagTestCase testCase)
	{
		if (testCase == null)
			throw new ArgumentNullException(nameof(testCase));
		if (string.IsNullOrWhiteSpace(testCase.Question))
			throw new ArgumentException("The test case must contain a question.", nameof(testCase));

		object searchResult = await semanticSearch.SearchAsync(testCase.Question, 0.3, 2);
		if (!(searchResult is IEnumerable retrievedChunks))
			throw new InvalidOperationException("SearchAsync must return a collection of chunks.");

		var chunks = retrievedChunks.Cast<object>()
			.Select(chunk => getChunkText(chunk) ?? string.Empty)
			.ToList();
		var errors = new List<string>();
		string answer = string.Empty;

		if (chunks.Count == 0)
		{
			if (testCase.ShouldBeAnswerable)
				errors.Add("An answerable question returned no chunks.");
		}
		else
		{
			// Keywords may be distributed across the retrieved chunks.
			var expectedKeywords = testCase.ExpectedKeywords
				?? Enumerable.Empty<string>();
			foreach (string keyword in expectedKeywords)
			{
				if (string.IsNullOrWhiteSpace(keyword))
				{
					errors.Add("Expected keywords must not be empty.");
					continue;
				}

				if (!chunks.Any(chunk => chunk.IndexOf(keyword, StringComparison.OrdinalIgnoreCase) >= 0))
					errors.Add($"Retrieved chunks do not contain the expected keyword: {keyword}");
			}

			answer = await generateAnswerAsync(testCase.Question, chunks.AsReadOnly())
				?? string.Empty;
            bool saysUnavailable = SaysUnavailable(answer);
			
            if (!testCase.ShouldBeAnswerable && !saysUnavailable)
			{
					errors.Add("An unanswerable question should not produce an answer.");
			}

			if (testCase.ShouldBeAnswerable)
			{
				if (string.IsNullOrWhiteSpace(answer) || saysUnavailable)
				{
					errors.Add("An answerable question did not produce an answer.");
				}
				else
				{
					foreach (string keyword in expectedKeywords)
					{
						if (!answer.Contains(keyword, StringComparison.OrdinalIgnoreCase))
						{
							errors.Add($"Generated answer is missing expected keyword: {keyword}");
						}
					}
				}
			}
		else if (!saysUnavailable)
		{
			errors.Add("An unanswerable question should not produce an answer.");
		}
		}

		return new EvaluationResult(chunks.AsReadOnly(), answer, errors.AsReadOnly());
	}

	public async Task<IReadOnlyList<EvaluationResult>> EvaluateAllAsync()
	{
		var results = new List<EvaluationResult>();
		foreach (var testCase in RagTestCases.GetAll())
		{
			var result = await EvaluateAsync(testCase);
			results.Add(result);
		}
		return results.AsReadOnly();
	}

	public sealed class EvaluationResult
	{
		public bool Passed => Errors.Count == 0;
		public IReadOnlyList<string> RetrievedChunks { get; }
		public string Answer { get; }
		public IReadOnlyList<string> Errors { get; }

		internal EvaluationResult(
			IReadOnlyList<string> retrievedChunks,
			string answer,
			IReadOnlyList<string> errors)
		{
			RetrievedChunks = retrievedChunks;
			Answer = answer;
			Errors = errors;
		}
	}

    private static bool SaysUnavailable(string answer)
{
    string[] indicators =
    {
        "the information is not available",
        "does not offer any information",
        "not enough information",
        "cannot answer",
        "does not provide information"
    };

    return indicators.Any(indicator =>
        answer.Contains(indicator, StringComparison.OrdinalIgnoreCase));
}
}
