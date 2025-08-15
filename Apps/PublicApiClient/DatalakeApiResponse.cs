using Microsoft.AspNetCore.Http;
using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;

namespace Datalake.PublicApiClient;

/// <summary>
/// Обертка над ответом
/// </summary>
/// <typeparam name="T">Тип ответа</typeparam>
public sealed class DatalakeApiResponse<T> : IAsyncDisposable
{
	private readonly HttpResponseMessage _response;
	private bool _consumed;

	public Type ExpectedType => typeof(T);
	public HttpStatusCode StatusCode => _response.StatusCode;
	public bool IsSuccessStatusCode => _response.IsSuccessStatusCode;
	public HttpResponseHeaders Headers => _response.Headers;
	public HttpContentHeaders ContentHeaders => _response.Content.Headers;
	public string? MediaType => _response.Content.Headers.ContentType?.MediaType;

	internal DatalakeApiResponse(HttpResponseMessage response) => _response = response;

	/// <summary>
	/// Получить непрочитанный поток (одноразовый захват)
	/// </summary>
	public async Task<Stream> AcquireStreamAsync(CancellationToken ct = default)
	{
		ThrowIfConsumed();
		_consumed = true;
		return await _response.Content.ReadAsStreamAsync(ct).ConfigureAwait(false);
	}

	/// <summary>
	/// Десериализовать по требованию (одноразовое потребление)
	/// </summary>
	public async Task<T?> ReadAsAsync(JsonSerializerOptions? options = null, CancellationToken ct = default)
	{
		ThrowIfConsumed();
		_consumed = true;
		await using var s = await _response.Content.ReadAsStreamAsync(ct).ConfigureAwait(false);
		return await JsonSerializer.DeserializeAsync<T>(s, options, ct).ConfigureAwait(false);
	}

	/// <summary>
	/// Проксирование в ASP.NET Core-респонс (с заголовками и статусом)
	/// </summary>
	public async Task ForwardToAsync(HttpResponse target, bool copyHeaders = true, CancellationToken ct = default)
	{
		ThrowIfConsumed();
		_consumed = true;

		target.StatusCode = (int)_response.StatusCode;

		if (copyHeaders)
			CopyResponseHeaders(_response, target);

		await using var src = await _response.Content.ReadAsStreamAsync(ct).ConfigureAwait(false);
		await src.CopyToAsync(target.Body, 64 * 1024, ct).ConfigureAwait(false);
		// Доп. Flush обычно не нужен; ASP.NET Core сам буферизует и флашит по мере необходимости.
	}

	public void EnsureSuccess()
	{
		if (!_response.IsSuccessStatusCode)
			throw new HttpRequestException($"Upstream returned {(int)_response.StatusCode} {_response.ReasonPhrase}.");
	}

	public ValueTask DisposeAsync()
	{
		_response.Dispose();
		return default;
	}

	private void ThrowIfConsumed()
	{
		if (_consumed)
			throw new InvalidOperationException("Response stream already consumed.");
	}

	private static void CopyResponseHeaders(HttpResponseMessage src, HttpResponse dst)
	{
		static bool IsHopByHop(string h) => h.Equals("Connection", StringComparison.OrdinalIgnoreCase)
			|| h.Equals("Keep-Alive", StringComparison.OrdinalIgnoreCase)
			|| h.Equals("Proxy-Authenticate", StringComparison.OrdinalIgnoreCase)
			|| h.Equals("Proxy-Authorization", StringComparison.OrdinalIgnoreCase)
			|| h.Equals("TE", StringComparison.OrdinalIgnoreCase)
			|| h.Equals("Trailer", StringComparison.OrdinalIgnoreCase)
			|| h.Equals("Transfer-Encoding", StringComparison.OrdinalIgnoreCase)
			|| h.Equals("Upgrade", StringComparison.OrdinalIgnoreCase);

		// Сначала копируем обычные заголовки ответа
		foreach (var header in src.Headers)
		{
			if (IsHopByHop(header.Key))
				continue;
			dst.Headers[header.Key] = header.Value.ToArray();
		}

		// Контент‑заголовки — аккуратно:
		// Content-Type и Content-Length лучше через свойства, остальное — в Headers.
		if (src.Content.Headers.ContentType is { } ct)
			dst.ContentType = ct.ToString();

		if (src.Content.Headers.ContentLength is long len)
			dst.ContentLength = len; // ставь только если не меняешь тело

		foreach (var header in src.Content.Headers)
		{
			if (IsHopByHop(header.Key))
				continue;
			if (string.Equals(header.Key, "Content-Type", StringComparison.OrdinalIgnoreCase))
				continue;
			if (string.Equals(header.Key, "Content-Length", StringComparison.OrdinalIgnoreCase))
				continue;

			dst.Headers[header.Key] = header.Value.ToArray();
		}
	}
}