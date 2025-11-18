using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Net.Mime;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Datalake.Gateway.Host.Services;

/// <summary>
/// Основа сервиса для прозрачного проксирования запросов в контроллерах Gateway API
/// </summary>
public abstract class ReverseProxyService(HttpClient httpClient)
{
	/// <summary>
	/// Настройки JSON
	/// </summary>
	protected virtual JsonSerializerOptions JsonSerializerOptions { get; } = new()
	{
		PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
		DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
	};

	/// <summary>
	/// Проксирование запроса без ответа
	/// </summary>
	public Task<ActionResult> ProxyAsync(
		HttpContext context,
		CancellationToken cancellationToken = default) => ProxyAsync(context, null, null, cancellationToken);

	/// <summary>
	/// Проксирование запроса с передачей ответа без обработки
	/// </summary>
	public Task<ActionResult<TResponse>> ProxyAsync<TResponse>(
		HttpContext context,
		CancellationToken cancellationToken = default) => ProxyAsync<TResponse>(context, null, null, cancellationToken);

	/// <summary>
	/// Проксирование запроса без ответа
	/// </summary>
	public Task<ActionResult> ProxyAsync(
		HttpContext context,
		object? body,
		CancellationToken cancellationToken = default) => ProxyAsync(context, body, null, cancellationToken);

	/// <summary>
	/// Проксирование запроса с передачей ответа без обработки
	/// </summary>
	public Task<ActionResult<TResponse>> ProxyAsync<TResponse>(
		HttpContext context,
		object? body,
		CancellationToken cancellationToken = default) => ProxyAsync<TResponse>(context, body, null, cancellationToken);

	/// <summary>
	/// Проксирование запроса без ответа
	/// </summary>
	public async Task<ActionResult> ProxyAsync(
		HttpContext context,
		object? body = null,
		Dictionary<string, string>? headers = null,
		CancellationToken cancellationToken = default)
	{
		HttpResponseMessage? response = null;
		try
		{
			response = await SendProxyRequestAsync(context, body, headers, cancellationToken);

			if (!response.IsSuccessStatusCode)
			{
				await HandleErrorResponse(response, context);
				return new EmptyResult();
			}

			await CopyResponseStreamAsync(response, context, cancellationToken);
			return new EmptyResult();
		}
		catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
		{
			response?.Dispose();
			throw;
		}
		catch
		{
			response?.Dispose();
			throw;
		}
	}

	/// <summary>
	/// Проксирование запроса с передачей ответа без обработки
	/// </summary>
	public async Task<ActionResult<TResponse>> ProxyAsync<TResponse>(
		HttpContext context,
		object? body = null,
		Dictionary<string, string>? headers = null,
		CancellationToken cancellationToken = default)
	{
		HttpResponseMessage? response = null;
		try
		{
			response = await SendProxyRequestAsync(context, body, headers, cancellationToken);

			if (!response.IsSuccessStatusCode)
			{
				await HandleErrorResponse(response, context);
				return new EmptyResult();
			}

			var result = await response.Content.ReadFromJsonAsync<TResponse>(cancellationToken: cancellationToken);
			return new ActionResult<TResponse>(result!);
		}
		catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
		{
			response?.Dispose();
			throw;
		}
		catch
		{
			response?.Dispose();
			throw;
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

		// Добавляем тело запроса (оптимизированная сериализация)
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

	private async Task<HttpResponseMessage> SendProxyRequestAsync(
		HttpContext context,
		object? body,
		Dictionary<string, string>? headers,
		CancellationToken cancellationToken)
	{
		var method = HttpMethod.Parse(context.Request.Method);
		var relativePath = GetRelativePath(context);
		var queryString = context.Request.QueryString.Value ?? string.Empty;

		var requestMessage = CreateHttpRequestMessage(method, $"{relativePath}{queryString}", body, headers, context);

		return await httpClient.SendAsync(requestMessage, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
	}

	private static async Task CopyResponseStreamAsync(
		HttpResponseMessage response,
		HttpContext context,
		CancellationToken cancellationToken)
	{
		// Устанавливаем статус и основные заголовки
		context.Response.StatusCode = (int)response.StatusCode;

		// Копируем только необходимые заголовки
		if (response.Content.Headers.ContentType is not null)
		{
			context.Response.ContentType = response.Content.Headers.ContentType.MediaType!;
		}

		if (response.Content.Headers.ContentLength.HasValue)
		{
			context.Response.ContentLength = response.Content.Headers.ContentLength.Value;
		}

		// Прямое копирование потока
		using var sourceStream = await response.Content.ReadAsStreamAsync(cancellationToken);
		await sourceStream.CopyToAsync(context.Response.Body, 81920, cancellationToken); // Увеличили буфер
	}

	// Оптимизированная обработка ошибок для потокового режима
	private static async Task HandleErrorResponse(HttpResponseMessage response, HttpContext context)
	{
		var statusCode = response.StatusCode;
		var content = await response.Content.ReadAsStringAsync();
		var contentType = response.Content.Headers.ContentType?.MediaType ?? "text/plain";

		context.Response.StatusCode = (int)statusCode;
		context.Response.ContentType = contentType;
		await context.Response.WriteAsync(content);
	}
}

/// <summary>
/// Исключение для прозрачного перенаправления ошибок
/// </summary>
public class ReverseProxyTransparentException(HttpStatusCode statusCode, string originalContent, string contentType)
	: Exception($"Proxy received error: {statusCode}")
{
	/// <summary>
	/// Код запроса
	/// </summary>
	public HttpStatusCode StatusCode { get; } = statusCode;

	/// <summary>
	/// Исходный контент
	/// </summary>
	public string OriginalContent { get; } = originalContent;

	/// <summary>
	/// Тип исходного контента
	/// </summary>
	public string ContentType { get; } = contentType;
}

/// <summary>
/// Middleware для обработки прозрачных ошибок прокси
/// </summary>
public class ReverseProxyTransparentExceptionMiddleware(RequestDelegate next, ILogger<ReverseProxyTransparentExceptionMiddleware> logger)
{
	/// <summary>
	/// Обработка запроса
	/// </summary>
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
	/// <summary>
	/// Подключение обработки ошибок прокси-запросов
	/// </summary>
	public static IApplicationBuilder UseReverseProxy(this IApplicationBuilder app)
	{
		app.UseMiddleware<ReverseProxyTransparentExceptionMiddleware>();

		return app;
	}
}
