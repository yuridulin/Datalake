using Microsoft.Extensions.Hosting;

namespace Datalake.Data.Application;

public static class Bootstrap
{
	public static IHostApplicationBuilder AddApplication(this IHostApplicationBuilder builder)
	{

		return builder;
	}
}
