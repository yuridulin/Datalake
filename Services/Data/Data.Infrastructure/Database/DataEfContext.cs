using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace Datalake.Data.Infrastructure.Database;

public class DataEfContext(DbContextOptions<DataEfContext> options) : DbContext(options)
{
	/// <summary>
	/// Конфигурация связей между таблицами БД
	/// </summary>
	protected override void OnModelCreating(ModelBuilder modelBuilder)
	{
		modelBuilder.HasDefaultSchema("data");

		// Автоматически применяет все классы конфигурации
		// из текущей сборки, реализующие IEntityTypeConfiguration<>
		modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
	}
}
