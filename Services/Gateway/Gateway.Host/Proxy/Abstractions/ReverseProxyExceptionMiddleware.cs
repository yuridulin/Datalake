namespace Datalake.Gateway.Host.Proxy.Abstractions;

/// <summary>
/// Middleware для обработки прозрачных ошибок прокси
/// </summary>
public class ReverseProxyExceptionMiddleware(ILogger<ReverseProxyExceptionMiddleware> logger) : IMiddleware
{
	/// <inheritdoc/>
	public async Task InvokeAsync(HttpContext context, RequestDelegate next)
	{
		try
		{
			await next(context);
		}
		catch (ReverseProxyException ex)
		{
			if (logger.IsEnabled(LogLevel.Debug))
				logger.LogDebug("Transparent proxy error: {StatusCode} - {ContentType}", ex.StatusCode, ex.ContentType);

			context.Response.StatusCode = (int)ex.StatusCode;
			context.Response.ContentType = ex.ContentType;
			await context.Response.WriteAsync(ex.OriginalContent);
		}
	}
}
