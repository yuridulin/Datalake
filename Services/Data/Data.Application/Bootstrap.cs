using Datalake.Data.Application.Services;
using Datalake.Shared.Application.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Reflection;

namespace Datalake.Data.Application;

public static class Bootstrap
{
	public static IHostApplicationBuilder AddApplication(this IHostApplicationBuilder builder)
	{
		// CQRS
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

		// Системы

		// Настройка
		builder.Services.AddHostedService<ApplicationStartService>();

		return builder;
	}
}
