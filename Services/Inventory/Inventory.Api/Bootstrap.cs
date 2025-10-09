using FluentValidation;
using Microsoft.Extensions.Hosting;
using System.Reflection;

namespace Datalake.Inventory.Api;

public static class Bootstrap
{
	public static IHostApplicationBuilder AddApi(this IHostApplicationBuilder builder)
	{
		builder.Services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

		return builder;
	}
}
