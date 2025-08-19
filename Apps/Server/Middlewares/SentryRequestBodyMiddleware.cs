using System.Text;
using System.Text.Json;

namespace Datalake.Server.Middlewares;

/// <summary>
/// Обработчик, добавляющий тело запроса в мета-данные Sentry
/// </summary>
public class SentryRequestBodyMiddleware : IMiddleware
{
	/// <inheritdoc/>
	public async Task InvokeAsync(HttpContext context, RequestDelegate next)
	{
		// Включаем буферизацию
		context.Request.EnableBuffering();

		string bodyAsText;

		using (var reader = new StreamReader(
			context.Request.Body,
			encoding: Encoding.UTF8,
			detectEncodingFromByteOrderMarks: false,
			bufferSize: 1024,
			leaveOpen: true))
		{
			bodyAsText = await reader.ReadToEndAsync();
			context.Request.Body.Position = 0;
		}

		// Пытаемся распарсить JSON и скрыть чувствительные поля
		try
		{
			var json = JsonSerializer.Deserialize<Dictionary<string, object>>(bodyAsText);
			if (json != null)
			{
				var sensitiveKeys = new[] { "password", "token", "secret", "authorization" };
				foreach (var key in sensitiveKeys)
				{
					if (json.ContainsKey(key))
						json[key] = "***MASKED***";
				}
				bodyAsText = JsonSerializer.Serialize(json);
			}
		}
		catch
		{
			// Не JSON — оставляем как есть
		}

		// Добавляем в Sentry scope
		SentrySdk.ConfigureScope(scope =>
		{
			scope.SetExtra("request_body", bodyAsText);
		});

		await next(context);
	}
}
