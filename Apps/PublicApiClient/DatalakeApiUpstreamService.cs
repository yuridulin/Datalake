using System.Net.Http.Json;
using System.Text.Json;

namespace Datalake.PublicApiClient;

public class DatalakeApiUpstreamService
{
	public static async Task<DatalakeApiResponse<TResponse>> PostAsync<TResponse, TRequest>(
		HttpClient http,
		string uri,
		TRequest payload,
		IDictionary<string, string>? extraHeaders = null,
		bool validateJsonContentType = true,
		bool ensureSuccess = false, // false — полезно для пасс-тру
		JsonSerializerOptions? jsonOptions = null,
		CancellationToken ct = default)
	{
		using var req = new HttpRequestMessage(HttpMethod.Post, uri)
		{
			Content = JsonContent.Create(payload, options: jsonOptions)
		};

		req.Headers.Accept.Clear();
		req.Headers.Accept.ParseAdd("application/json"); // договор о типе
		req.VersionPolicy = HttpVersionPolicy.RequestVersionOrHigher;

		if (extraHeaders is not null)
		{
			foreach (var kv in extraHeaders)
				req.Headers.TryAddWithoutValidation(kv.Key, kv.Value);
		}

		var res = await http.SendAsync(req, HttpCompletionOption.ResponseHeadersRead, ct).ConfigureAwait(false);

		if (ensureSuccess)
		{
			// Для внутренних вызовов — ранняя проверка
			if (!res.IsSuccessStatusCode)
			{
				string? body = null;
				try
				{
					body = await res.Content.ReadAsStringAsync(ct).ConfigureAwait(false);
					if (body?.Length > 2048)
						body = body[..2048] + "…";
				}
				catch { /* best effort */ }
				res.Dispose();
				throw new HttpRequestException(
					$"HTTP {(int)res.StatusCode} {res.ReasonPhrase}. " +
					(body is null ? "No error body." : $"Error body: {body}"));
			}
		}

		if (validateJsonContentType)
		{
			var mt = res.Content.Headers.ContentType?.MediaType;
			if (mt is string s && !s.Equals("application/json", StringComparison.OrdinalIgnoreCase) &&
					!s.EndsWith("+json", StringComparison.OrdinalIgnoreCase))
			{
				res.Dispose();
				throw new InvalidOperationException($"Ожидался JSON-ответ, получен '{s}'.");
			}
		}

		return new DatalakeApiResponse<TResponse>(res);
	}
}
