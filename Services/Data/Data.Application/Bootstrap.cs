using Datalake.Shared.Application.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Reflection;

namespace Datalake.Data.Application;

public static class Bootstrap
{
	public static IHostApplicationBuilder AddApplication(this IHostApplicationBuilder builder)
	{
		// черная магия для регистрации обработчиков
		builder.Services.Scan(scan => scan
			.FromAssemblies(Assembly.GetExecutingAssembly())
			.AddClasses(classes => classes.AssignableTo(typeof(ICommandHandler<,>)))
				.AsImplementedInterfaces()
				.WithScopedLifetime());
		builder.Services.Scan(scan => scan
			.FromAssemblies(Assembly.GetExecutingAssembly())
			.AddClasses(classes => classes.AssignableTo(typeof(IQueryHandler<,>)))
				.AsImplementedInterfaces()
				.WithScopedLifetime());

		return builder;
	}
}
