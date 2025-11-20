using Datalake.Gateway.Application.Interfaces;
using Datalake.Gateway.Host.Interfaces;
using Datalake.Shared.Application.Exceptions;
using Datalake.Shared.Hosting.Constants;

namespace Datalake.Gateway.Host.Middlewares;

/// <summary>
/// Перехватчик, проверяющий сессию и выставляющий необходимые заголовки для сервисов
/// </summary>
public class SessionMiddleware(
	ISessionTokenExtractor tokenExtractor,
	ISessionsService sessionsService,
	ILogger<SessionMiddleware> logger) : IMiddleware
{
	/// <inheritdoc/>
	public async Task InvokeAsync(HttpContext context, RequestDelegate next)
	{
		// Пропускаем запросы health-check
		if (context.Request.Path.StartsWithSegments("/health"))
		{
			await next(context);
			return;
		}

		// Пропускаем аутентификацию для эндпоинтов сессий
		if (context.Request.Path.StartsWithSegments("/api/v1/gateway/sessions") &&
			context.Request.Method != "GET" && context.Request.Method != "DELETE")
		{
			await next(context);
			return;
		}

		try
		{
			// Извлекаем токен сессии
			var sessionToken = tokenExtractor.ExtractToken(context);

			// Получаем информацию о сессии
			var sessionInfo = await sessionsService.GetAsync(sessionToken);

			// Добавляем UserGuid в заголовки для downstream сервисов
			context.Request.Headers[Headers.UserGuidHeader] = sessionInfo.UserGuid.ToString();

			await next(context);
		}
		catch (UnauthenticatedException ex)
		{
			if (logger.IsEnabled(LogLevel.Warning))
				logger.LogWarning("Неавторизованный доступ: {Message}", ex.Message);

			context.Response.StatusCode = 401;
			await context.Response.WriteAsJsonAsync(new { error = ex.Message });
		}
		catch (Exception ex)
		{
			logger.LogError(ex, "Ошибка при проверке сессии");
			context.Response.StatusCode = 500;
			await context.Response.WriteAsJsonAsync(new { error = "Ошибка при проверке сессии" });
		}
	}
}
