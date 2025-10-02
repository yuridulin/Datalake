using Datalake.Inventory.Application.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Reflection;

namespace Datalake.Inventory.Application.Bootstrap;

public static class ApplicationBootstrapExtensions
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
