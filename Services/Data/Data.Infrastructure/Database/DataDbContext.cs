using Datalake.Domain.Entities;
using Datalake.Domain.ValueObjects;
using Datalake.Shared.Infrastructure.Schema;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using System.Reflection;

namespace Datalake.Data.Infrastructure.Database;

/// <summary>
/// 
/// </summary>
/// Add-Migration Initial -Context Datalake.Data.Infrastructure.Database.DataDbContext -OutputDir Database\Migrations
/// Remove-Migration -Context Datalake.Data.Infrastructure.Database.DataDbContext
public class DataDbContext : DbContext
{
	public DataDbContext(DbContextOptions<DataDbContext> options) : base(options)
	{
		// Отключаем отслеживание изменений для повышения производительности
		ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
	}

	/// <summary>
	/// Конфигурация связей между таблицами БД
	/// </summary>
	protected override void OnModelCreating(ModelBuilder modelBuilder)
	{
		modelBuilder.HasDefaultSchema("data");

		// Автоматически применяет все классы конфигурации
		// из текущей сборки, реализующие IEntityTypeConfiguration<>
		modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

		// Переключаем таблицы в режим представления, чтобы этот контекст не пытался ими управлять
		modelBuilder.Entity<Source>(entity => entity.ToView(InventorySchema.Sources.Name, InventorySchema.Name));
		modelBuilder.Entity<Tag>(entity => entity.ToView(InventorySchema.Tags.Name, InventorySchema.Name));
		modelBuilder.Entity<TagInput>(entity => entity.ToView(InventorySchema.TagInputs.Name, InventorySchema.Name));
		modelBuilder.Entity<TagThreshold>(entity => entity.ToView(InventorySchema.TagThresholds.Name, InventorySchema.Name));
	}

	public virtual DbSet<Source> Sources { get; set; }

	public virtual DbSet<Tag> Tags { get; set; }

	public virtual DbSet<TagInput> TagInputs { get; set; }

	public virtual DbSet<TagThreshold> TagThresholds { get; set; }

	public virtual DbSet<TagHistory> TagsHistory { get; set; }
}

public class DataDbContextFactory : IDesignTimeDbContextFactory<DataDbContext>
{
	public DataDbContext CreateDbContext(string[] args)
	{
		var environment = "Migrations";

		var storage = Path.Combine(Directory.GetCurrentDirectory(), "storage", "config");
		var config = new ConfigurationBuilder()
			.SetBasePath(storage)
			.AddJsonFile("appsettings.json")
			.AddJsonFile($"appsettings.{environment}.json", optional: true)
			.Build();

		var optionsBuilder = new DbContextOptionsBuilder<DataDbContext>();
		optionsBuilder.UseNpgsql(config.GetConnectionString("Default"), options =>
		{
			options.MigrationsAssembly($"{nameof(Datalake)}.{nameof(Data)}.{nameof(Infrastructure)}");
		});

		return new(optionsBuilder.Options);
	}
}
