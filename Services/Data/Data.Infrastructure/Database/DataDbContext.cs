using Datalake.Domain.Entities;
using Datalake.Domain.ValueObjects;
using Datalake.Shared.Infrastructure;
using Datalake.Shared.Infrastructure.Schema;
using Microsoft.EntityFrameworkCore;
using static Datalake.Shared.Infrastructure.ConfigurationsApplyHelper;

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
		modelBuilder.HasDefaultSchema(DataSchema.Name);

		modelBuilder.ApplyConfigurations(new()
		{
			AccessRules = TableAccess.Read,
			Audit = TableAccess.Read,
			Blocks = TableAccess.Read,
			BlocksProperties = TableAccess.Read,
			BlocksTags = TableAccess.Read,
			CalculatedAccessRules = TableAccess.Read,
			Settings = TableAccess.Read,
			Sources = TableAccess.Read,
			Tags = TableAccess.Read,
			TagsHistory = TableAccess.Write,
			TagsInputs = TableAccess.Read,
			TagsThresholds = TableAccess.Read,
			UserGroups = TableAccess.Read,
			UserGroupsRelations = TableAccess.Read,
			Users = TableAccess.Read,
			UserSessions = TableAccess.Read,
		});
	}

	public virtual DbSet<TagHistoryValue> TagsHistory { get; set; }

	public virtual DbSet<Source> Sources { get; set; }

	public virtual DbSet<Tag> Tags { get; set; }

	public virtual DbSet<TagInput> TagInputs { get; set; }

	public virtual DbSet<TagThreshold> TagThresholds { get; set; }
}
