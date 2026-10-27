using System.Text;
using System.Text.Json;

	public sealed class OllamaChatService
	{
		private readonly HttpClient _httpClient;
		private readonly string _model;
		private readonly Uri _chatEndpoint;

		// The caller owns the HttpClient and configures its timeout.
		public OllamaChatService(
			HttpClient httpClient,
			string model = "qwen2.5:3b",
			string baseUrl = "http://localhost:11434")
		{
			_httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));

			if (string.IsNullOrWhiteSpace(model))
				throw new ArgumentException("A model name is required.", nameof(model));
			if (string.IsNullOrWhiteSpace(baseUrl))
				throw new ArgumentException("An Ollama base URL is required.", nameof(baseUrl));

			_model = model;
			_chatEndpoint = new Uri(baseUrl.TrimEnd('/') + "/api/chat", UriKind.Absolute);
		}

		public async Task<string> GenerateAnswerAsync(
			string question,
			List<string> relevantChunks,
			CancellationToken cancellationToken = default)
		{
			if (string.IsNullOrWhiteSpace(question))
				throw new ArgumentException("A question is required.", nameof(question));
			if (relevantChunks == null)
				throw new ArgumentNullException(nameof(relevantChunks));

			var context = string.Join("\n\n---\n\n",
				relevantChunks.Where(chunk => !string.IsNullOrWhiteSpace(chunk)));

			var request = new
			{
				model = _model,
				stream = false,
				options = new { temperature = 0 },
				messages = new[]
				{
					new
					{
						role = "system",
						 content =
                "You are a support assistant. Answer the question using the provided context. " +
                "Base your answer only on the context and do not invent facts. " +
                "The question may be phrased as a statement or symptom; in that case, " +
                "explain the cause and the recommended steps from the context. " +
                "If the context is clearly unrelated to the question, say that you don't know." +
				"Keep the answer short (max. 3 sentences). Use only steps that are explicitly mentioned " +
				"in the context. Do not add extra steps, examples or advice of your own. "

					},
					new
					{
						role = "user",
						content = "Context:\n" + context + "\n\nQuestion:\n" + question
					}
				}
			};

			using (var body = new StringContent(
				JsonSerializer.Serialize(request), Encoding.UTF8, "application/json"))
			using (var response = await _httpClient.PostAsync(
				_chatEndpoint, body, cancellationToken).ConfigureAwait(false))
			{
				response.EnsureSuccessStatusCode();

				using (var stream = await response.Content.ReadAsStreamAsync().ConfigureAwait(false))
				using (var document = await JsonDocument.ParseAsync(
					stream, cancellationToken: cancellationToken).ConfigureAwait(false))
				{
					if (!document.RootElement.TryGetProperty("message", out var message) ||
						message.ValueKind != JsonValueKind.Object ||
						!message.TryGetProperty("content", out var content) ||
						content.ValueKind != JsonValueKind.String)
					{
						throw new InvalidOperationException("Ollama returned no assistant message.");
					}

					return content.GetString() ?? string.Empty;
				}
			}
		}
	}

