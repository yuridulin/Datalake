using Datalake.Shared.Application.Attributes;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Datalake.Data.Infrastructure.Database.Services;

/// <summary>
/// Настройка БД
/// </summary>
[Singleton]
public class DbInitializer(
	IServiceScopeFactory serviceScopeFactory,
	ILogger<DbInitializer> logger) : BackgroundService
{
	protected override async Task ExecuteAsync(CancellationToken stoppingToken)
	{
		logger.LogInformation("Настройка БД");

		try
		{
			using var serviceScope = serviceScopeFactory.CreateScope();

			// выполняем миграции через EF
			var context = serviceScope.ServiceProvider.GetRequiredService<DataDbContext>();
			await context.Database.MigrateAsync(stoppingToken);
		}
		catch (Exception ex)
		{
			logger.LogError(ex, "Настройка БД не выполнена!");
			throw;
		}
	}
}
