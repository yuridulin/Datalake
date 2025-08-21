using System.Net.Http.Json;
using System.Text.Json;

namespace Datalake.PublicApiClient;

/// <summary>
/// Сервис перенаправления
/// </summary>
public static class ApiUpstreamService
{
	/// <summary>
	/// GET
	/// </summary>
	/// <typeparam name="TResponse">Тип ответа</typeparam>
	/// <param name="http">Настроенный http клиент</param>
	/// <param name="uri">Относительный путь</param>
	/// <param name="extraHeaders">Дополнительные заголовки</param>
	/// <param name="validateJsonContentType">Нужно ли проверять, что ответ является JSON</param>
	/// <param name="ensureSuccess">Нужно ли проверять, что статус ответа успешный</param>
	/// <param name="ct">Токен отмены операции</param>
	/// <returns>Универсальный типизированный ответ</returns>
	/// <exception cref="HttpRequestException"></exception>
	/// <exception cref="InvalidOperationException"></exception>
	public static Task<ApiUpstreamResponse<TResponse>> GetAsync<TResponse>(
		HttpClient http,
		string uri,
		IDictionary<string, string>? extraHeaders = null,
		bool validateJsonContentType = true,
		bool ensureSuccess = false,
		CancellationToken ct = default)
		=> SendAsync<object?, TResponse>(http, HttpMethod.Get, uri, null, extraHeaders, validateJsonContentType, ensureSuccess, null, ct);

	/// <summary>
	/// POST
	/// </summary>
	/// <typeparam name="TRequest">Тип запроса</typeparam>
	/// <typeparam name="TResponse">Тип ответа</typeparam>
	/// <param name="http">Настроенный http клиент</param>
	/// <param name="uri">Относительный путь</param>
	/// <param name="payload">Объект запроса</param>
	/// <param name="jsonOptions">Настройки сериализации для тела запроса</param>
	/// <param name="extraHeaders">Дополнительные заголовки</param>
	/// <param name="validateJsonContentType">Нужно ли проверять, что ответ является JSON</param>
	/// <param name="ensureSuccess">Нужно ли проверять, что статус ответа успешный</param>
	/// <param name="ct">Токен отмены операции</param>
	/// <returns>Универсальный типизированный ответ</returns>
	/// <exception cref="HttpRequestException"></exception>
	/// <exception cref="InvalidOperationException"></exception>
	public static Task<ApiUpstreamResponse<TResponse>> PostAsync<TRequest, TResponse>(
		HttpClient http,
		string uri,
		TRequest payload,
		IDictionary<string, string>? extraHeaders = null,
		bool validateJsonContentType = true,
		bool ensureSuccess = false,
		JsonSerializerOptions? jsonOptions = null,
		CancellationToken ct = default)
		=> SendAsync<TRequest, TResponse>(http, HttpMethod.Post, uri, payload, extraHeaders, validateJsonContentType, ensureSuccess, jsonOptions, ct);

	/// <summary>
	///
	/// </summary>
	/// <typeparam name="TRequest">Тип запроса</typeparam>
	/// <typeparam name="TResponse">Тип ответа</typeparam>
	/// <param name="http">Настроенный http клиент</param>
	/// <param name="uri">Относительный путь</param>
	/// <param name="payload">Объект запроса</param>
	/// <param name="jsonOptions">Настройки сериализации для тела запроса</param>
	/// <param name="extraHeaders">Дополнительные заголовки</param>
	/// <param name="validateJsonContentType">Нужно ли проверять, что ответ является JSON</param>
	/// <param name="ensureSuccess">Нужно ли проверять, что статус ответа успешный</param>
	/// <param name="ct">Токен отмены операции</param>
	/// <returns>Универсальный типизированный ответ</returns>
	/// <exception cref="HttpRequestException"></exception>
	/// <exception cref="InvalidOperationException"></exception>
	public static Task<ApiUpstreamResponse<TResponse>> PutAsync<TRequest, TResponse>(
		HttpClient http,
		string uri,
		TRequest payload,
		IDictionary<string, string>? extraHeaders = null,
		bool validateJsonContentType = true,
		bool ensureSuccess = false,
		JsonSerializerOptions? jsonOptions = null,
		CancellationToken ct = default)
		=> SendAsync<TRequest, TResponse>(http, HttpMethod.Put, uri, payload, extraHeaders, validateJsonContentType, ensureSuccess, jsonOptions, ct);

	/// <summary>
	/// Встречается реже, но DELETE c телом допустим. Сделай перегрузку без payload при необходимости.
	/// </summary>
	/// <typeparam name="TRequest">Тип запроса</typeparam>
	/// <typeparam name="TResponse">Тип ответа</typeparam>
	/// <param name="http">Настроенный http клиент</param>
	/// <param name="uri">Относительный путь</param>
	/// <param name="payload">Объект запроса</param>
	/// <param name="jsonOptions">Настройки сериализации для тела запроса</param>
	/// <param name="extraHeaders">Дополнительные заголовки</param>
	/// <param name="validateJsonContentType">Нужно ли проверять, что ответ является JSON</param>
	/// <param name="ensureSuccess">Нужно ли проверять, что статус ответа успешный</param>
	/// <param name="ct">Токен отмены операции</param>
	/// <returns>Универсальный типизированный ответ</returns>
	/// <exception cref="HttpRequestException"></exception>
	/// <exception cref="InvalidOperationException"></exception>
	public static Task<ApiUpstreamResponse<TResponse>> DeleteAsync<TRequest, TResponse>(
		HttpClient http,
		string uri,
		TRequest? payload = default,
		IDictionary<string, string>? extraHeaders = null,
		bool validateJsonContentType = true,
		bool ensureSuccess = false,
		JsonSerializerOptions? jsonOptions = null,
		CancellationToken ct = default)
		=> SendAsync<TRequest?, TResponse>(http, HttpMethod.Delete, uri, payload, extraHeaders, validateJsonContentType, ensureSuccess, jsonOptions, ct);

	/// <summary>
	/// Общий ядро-метод
	/// </summary>
	/// <typeparam name="TRequest"></typeparam>
	/// <typeparam name="TResponse"></typeparam>
	/// <param name="http"></param>
	/// <param name="method"></param>
	/// <param name="uri"></param>
	/// <param name="payload"></param>
	/// <param name="extraHeaders"></param>
	/// <param name="validateJsonContentType"></param>
	/// <param name="ensureSuccess"></param>
	/// <param name="jsonOptions"></param>
	/// <param name="ct"></param>
	/// <returns></returns>
	/// <exception cref="HttpRequestException"></exception>
	/// <exception cref="InvalidOperationException"></exception>
	private static async Task<ApiUpstreamResponse<TResponse>> SendAsync<TRequest, TResponse>(
		HttpClient http,
		HttpMethod method,
		string uri,
		TRequest? payload,
		IDictionary<string, string>? extraHeaders,
		bool validateJsonContentType,
		bool ensureSuccess,
		JsonSerializerOptions? jsonOptions,
		CancellationToken ct)
	{
		using var req = new HttpRequestMessage(method, uri);

		// Тело только если метод допускает и payload != null
		if ((method == HttpMethod.Post || method == HttpMethod.Put || method == HttpMethod.Delete) && payload is not null)
		{
			req.Content = JsonContent.Create(payload, options: jsonOptions);
		}

		req.Headers.Accept.Clear();
		req.Headers.Accept.ParseAdd("application/json");
		req.VersionPolicy = HttpVersionPolicy.RequestVersionOrHigher;

		if (extraHeaders is not null)
		{
			foreach (var kv in extraHeaders)
				req.Headers.TryAddWithoutValidation(kv.Key, kv.Value);
		}

		var res = await http.SendAsync(req, HttpCompletionOption.ResponseHeadersRead, ct).ConfigureAwait(false);

		if (ensureSuccess && !res.IsSuccessStatusCode)
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

		return new ApiUpstreamResponse<TResponse>(res);
	}
}