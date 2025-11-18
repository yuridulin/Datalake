using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

namespace Datalake.Shared.Hosting.Middlewares;

/// <summary>
/// Включение политики CORS для сообщений об ошибках
/// </summary>
public static class EnsureCorsMiddleware
{
	/// <summary>
	/// Включение политики CORS для сообщений об ошибках
	/// </summary>
	public static IApplicationBuilder UseSharedCorsOnError(this IApplicationBuilder builder)
	{
		return builder.Use(async (httpContext, next) =>
		{
			var corsHeaders = new HeaderDictionary();
			foreach (var pair in httpContext.Response.Headers)
			{
				if (!pair.Key.StartsWith("access-control-", StringComparison.InvariantCultureIgnoreCase))
				{ continue; }
				corsHeaders[pair.Key] = pair.Value;
			}

			httpContext.Response.OnStarting(o =>
			{
				var ctx = (HttpContext)o;
				var headers = ctx.Response.Headers;
				foreach (var pair in corsHeaders)
				{
					if (headers.ContainsKey(pair.Key))
					{ continue; }
					headers.Append(pair.Key, pair.Value);
				}
				return Task.CompletedTask;
			}, httpContext);

			await next();
		});
	}
}
