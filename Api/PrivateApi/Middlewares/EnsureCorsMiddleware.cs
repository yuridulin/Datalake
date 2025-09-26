using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

namespace Datalake.PrivateApi.Middlewares;

public static class EnsureCorsMiddleware
{
	public static IApplicationBuilder EnsureCorsMiddlewareOnError(this IApplicationBuilder builder)
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
