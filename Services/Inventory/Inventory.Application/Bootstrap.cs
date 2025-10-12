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

		builder.Services.AddSingleton<ApplicationStartService>();
		builder.Services.AddHostedService(provider => provider.GetRequiredService<ApplicationStartService>());

		builder.Services.AddSingleton<IUserAccessCalculationService, UserAccessCalculationService>();
		builder.Services.AddSingleton<IUserAccessSynchronizationService, UserAccessSynchronizationService>();

		return builder;
	}
}
