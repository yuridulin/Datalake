using Datalake.Shared.Infrastructure.Database;
using Datalake.Shared.Infrastructure.Database.Schema;
using Microsoft.EntityFrameworkCore;

namespace Datalake.Data.Infrastructure.Database;

/// <summary>
/// Контекст для работы с данными тегов
/// </summary>
/// Add-Migration NAME -Context Datalake.Data.Infrastructure.Database.DataDbContext -OutputDir Database\Migrations
/// Remove-Migration -Context Datalake.Data.Infrastructure.Database.DataDbContext
public class DataDbContext(DbContextOptions<DataDbContext> options) : AbstractDbContext(options)
{
	public override string DefaultSchema { get; } = DataSchema.Name;

	public override DatabaseTableAccessConfiguration TableAccessConfiguration { get; } = new()
	{
		AccessRules = DatabaseTableAccess.Read,
		Audit = DatabaseTableAccess.Read,
		Blocks = DatabaseTableAccess.Read,
		BlocksProperties = DatabaseTableAccess.Read,
		BlocksTags = DatabaseTableAccess.Read,
		CalculatedAccessRules = DatabaseTableAccess.Read,
		Settings = DatabaseTableAccess.Read,
		Sources = DatabaseTableAccess.Read,
		Tags = DatabaseTableAccess.Read,
		TagsValues = DatabaseTableAccess.Write,
		TagsInputs = DatabaseTableAccess.Read,
		TagsThresholds = DatabaseTableAccess.Read,
		UserGroups = DatabaseTableAccess.Read,
		UserGroupsRelations = DatabaseTableAccess.Read,
		Users = DatabaseTableAccess.Read,
		UserSessions = DatabaseTableAccess.Read,
	};

	protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
	{
		// Отключаем отслеживание изменений для повышения производительности
		optionsBuilder.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
	}
}
