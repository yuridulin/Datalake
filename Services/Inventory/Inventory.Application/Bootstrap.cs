using Datalake.Inventory.Application.Interfaces;
using Datalake.Inventory.Application.Services;
using Datalake.Shared.Application.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Reflection;

namespace Datalake.Inventory.Application;

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
		builder.Services.AddSingleton<IUserAccessCalculationService, UserAccessCalculationService>();
		builder.Services.AddSingleton<IUserAccessSynchronizationService, UserAccessSynchronizationService>();

		// Настройка
		builder.Services.AddHostedService<ApplicationStartService>();

		return builder;
	}
}
