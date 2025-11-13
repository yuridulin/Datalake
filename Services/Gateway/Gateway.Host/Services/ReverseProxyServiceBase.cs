using Datalake.Shared.Application.Attributes;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Net;
using System.Net.Mime;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Datalake.Gateway.Host.Services;

[Singleton]
public class DataReverseProxyService(
	IHttpClientFactory httpClientFactory,
	ILogger<DataReverseProxyService> logger)
	: ReverseProxyService(httpClientFactory.CreateClient("Data"), logger)
{
}

/// <summary>
/// Основа сервиса для прозрачного проксирования запросов в контроллерах Gateway API
/// </summary>
public abstract class ReverseProxyService(HttpClient httpClient, ILogger logger)
{
	protected virtual JsonSerializerOptions JsonSerializerOptions { get; } = new()
	{
		PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
		DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
	};

	public async Task<ActionResult<TResponse>> ProxyAsync<TResponse>(
		HttpContext context,
		object? body = null,
		Dictionary<string, string>? headers = null,
		CancellationToken cancellationToken = default)
	{
		try
		{
			var method = HttpMethod.Parse(context.Request.Method);
			var relativePath = GetRelativePath(context);
			var queryString = context.Request.QueryString.Value ?? string.Empty;

			var response = await SendProxyRequestAsync(method, $"{relativePath}{queryString}", body, headers, context, cancellationToken);
			var result = await ProcessResponseAsync<TResponse>(response);
			return new OkObjectResult(result);
		}
		catch (ReverseProxyTransparentException ex)
		{
			return new ObjectResult(ex.OriginalContent) { StatusCode = (int)ex.StatusCode };
		}
	}

	public async Task<ActionResult<TResponse>> ProxyStreamAsync<TResponse>(
		HttpContext context,
		object? body = null,
		Dictionary<string, string>? headers = null,
		CancellationToken cancellationToken = default)
	{
		try
		{
			var method = HttpMethod.Parse(context.Request.Method);
			var relativePath = GetRelativePath(context);
			var queryString = context.Request.QueryString.Value ?? string.Empty;

			var response = await SendProxyRequestAsync(method, $"{relativePath}{queryString}", body, headers, context, cancellationToken, true);
			return await ReturnStreamResponse(response, context);
		}
		catch (ReverseProxyTransparentException ex)
		{
			return new ObjectResult(ex.OriginalContent) { StatusCode = (int)ex.StatusCode };
		}
	}

	private async Task<HttpResponseMessage> SendProxyRequestAsync(
		HttpMethod method,
		string relativePath,
		object? body,
		Dictionary<string, string>? headers,
		HttpContext context,
		CancellationToken cancellationToken,
		bool headersOnly = false)
	{
		var absoluteUri = new Uri(httpClient.BaseAddress ?? throw new Exception("Базовый адрес не задан"), relativePath);

		HttpResponseMessage response = null!;
		Stopwatch? stopwatch = null;

		if (logger.IsEnabled(LogLevel.Debug))
			stopwatch = Stopwatch.StartNew();

		try
		{
			if (logger.IsEnabled(LogLevel.Debug))
				logger.LogDebug("Proxy {method} request to {uri}", method, absoluteUri);

			var requestMessage = CreateHttpRequestMessage(method, relativePath, body, headers, context);

			response = headersOnly
					? await httpClient.SendAsync(requestMessage, HttpCompletionOption.ResponseHeadersRead, cancellationToken)
					: await httpClient.SendAsync(requestMessage, cancellationToken);

			if (!response.IsSuccessStatusCode)
			{
				await ThrowTransparentError(response);
			}

			if (logger.IsEnabled(LogLevel.Debug))
				logger.LogDebug("Proxy {method} request to {uri} ... OK: {ms} ms", method, absoluteUri, stopwatch?.ElapsedMilliseconds);

			return response;
		}
		catch (Exception exception)
		{
			if (logger.IsEnabled(LogLevel.Debug))
				logger.LogDebug(exception, "Proxy {method} request to {uri}... FAILED after {ms} ms", method, absoluteUri, stopwatch?.ElapsedMilliseconds);

			response?.Dispose();
			throw;
		}
		finally
		{
			if (logger.IsEnabled(LogLevel.Debug))
				stopwatch?.Stop();
		}
	}

	private HttpRequestMessage CreateHttpRequestMessage(
		HttpMethod method,
				string relativePath,
		object? body,
		Dictionary<string, string>? headers,
		HttpContext context)
	{
		var requestMessage = new HttpRequestMessage
		{
			Method = method,
			RequestUri = new Uri(relativePath, UriKind.Relative)
		};

		// Добавляем тело запроса
		if (body != null)
		{
			requestMessage.Content = new StringContent(
				JsonSerializer.Serialize(body, JsonSerializerOptions),
				Encoding.UTF8,
				MediaTypeNames.Application.Json);
		}

		// Копируем авторизацию из оригинального запроса
		if (context.Request.Headers.TryGetValue("Authorization", out var auth))
		{
			requestMessage.Headers.TryAddWithoutValidation("Authorization", auth.ToString());
		}

		// Добавляем дополнительные заголовки
		if (headers != null)
		{
			foreach (var kv in headers)
			{
				requestMessage.Headers.TryAddWithoutValidation(kv.Key, kv.Value);
			}
		}

		return requestMessage;
	}

	private async Task<T> ProcessResponseAsync<T>(HttpResponseMessage response)
	{
		var contentString = await response.Content.ReadAsStringAsync();

		if (typeof(T) == typeof(string))
			return (T)(object)contentString;

		return string.IsNullOrEmpty(contentString)
			? default!
			: JsonSerializer.Deserialize<T>(contentString, JsonSerializerOptions)!;
	}

	private static async Task ThrowTransparentError(HttpResponseMessage response)
	{
		var statusCode = response.StatusCode;
		var content = await response.Content.ReadAsStringAsync();
		var contentType = response.Content.Headers.ContentType?.MediaType ?? "text/plain";

		throw new ReverseProxyTransparentException(statusCode, content, contentType);
	}

	private static string GetRelativePath(HttpContext context)
	{
		// Получаем полный путь из запроса
		var fullPath = context.Request.Path.Value ?? string.Empty;

		// Находим базовый путь контроллера из метаданных маршрута
		var endpoint = context.GetEndpoint();
		if (endpoint?.Metadata.GetMetadata<RouteAttribute>() is RouteAttribute routeAttr)
		{
			var controllerRoute = routeAttr.Template ?? string.Empty;
			if (!string.IsNullOrEmpty(controllerRoute) && fullPath.StartsWith(controllerRoute))
			{
				// Убираем базовый путь контроллера из полного пути
				return fullPath[controllerRoute.Length..].TrimStart('/');
			}
		}

		return fullPath.TrimStart('/');
	}

	private static async Task<FileStreamResult> ReturnStreamResponse(HttpResponseMessage response, HttpContext context)
	{
		var stream = await response.Content.ReadAsStreamAsync();
		var contentType = response.Content.Headers.ContentType?.MediaType ?? "application/json";

		context.Response.StatusCode = (int)response.StatusCode;
		return new FileStreamResult(stream, contentType);
	}
}

/// <summary>
/// Исключение для прозрачного перенаправления ошибок
/// </summary>
public class ReverseProxyTransparentException(HttpStatusCode statusCode, string originalContent, string contentType)
	: Exception($"Proxy received error: {statusCode}")
{
	public HttpStatusCode StatusCode { get; } = statusCode;
	public string OriginalContent { get; } = originalContent;
	public string ContentType { get; } = contentType;
}

/// <summary>
/// Middleware для обработки прозрачных ошибок прокси
/// </summary>
public class ReverseProxyTransparentExceptionMiddleware(RequestDelegate next, ILogger<ReverseProxyTransparentExceptionMiddleware> logger)
{
	public async Task InvokeAsync(HttpContext context)
	{
		try
		{
			await next(context);
		}
		catch (ReverseProxyTransparentException ex)
		{
			if (logger.IsEnabled(LogLevel.Debug))
				logger.LogDebug("Transparent proxy error: {StatusCode} - {ContentType}", ex.StatusCode, ex.ContentType);

			context.Response.StatusCode = (int)ex.StatusCode;
			context.Response.ContentType = ex.ContentType;
			await context.Response.WriteAsync(ex.OriginalContent);
		}
	}
}

/// <summary>
/// Вспомогательный класс для регистрации обработчика ошибок в DI
/// </summary>
public static class ReverseProxyDependencyInjections
{
	public static IApplicationBuilder UseReverseProxy(this IApplicationBuilder app)
	{
		app.UseMiddleware<ReverseProxyTransparentExceptionMiddleware>();

		return app;
	}
}
