using Datalake.Domain.Entities;
using Datalake.Domain.ValueObjects;
using Datalake.Shared.Infrastructure;
using Datalake.Shared.Infrastructure.Configurations;
using Microsoft.EntityFrameworkCore;

namespace Datalake.Data.Infrastructure.Database;

/// <summary>
/// Контекст для работы с данными тегов
/// </summary>
/// Add-Migration NAME -Context Datalake.Data.Infrastructure.Database.DataDbContext -OutputDir Database\Migrations
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

		modelBuilder.ApplyConfigurations(new()
		{
			AccessRules = true,
			Audit = true,
			Blocks = true,
			BlocksProperties = true,
			BlocksTags = true,
			Settings = true,
			Sources = true,
			Tags = true,
			TagsHistory = false,
			TagsInputs = true,
			TagsThresholds = true,
			UserGroups = true,
			UserGroupsRelations = true,
			Users = true,
		});
	}

	public virtual DbSet<TagHistory> TagsHistory { get; set; }

	public virtual DbSet<Source> Sources { get; set; }

	public virtual DbSet<Tag> Tags { get; set; }

	public virtual DbSet<TagInput> TagInputs { get; set; }

	public virtual DbSet<TagThreshold> TagThresholds { get; set; }
}
