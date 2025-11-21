using Datalake.Shared.Infrastructure.Database;
using Datalake.Shared.Infrastructure.Database.Schema;
using Microsoft.EntityFrameworkCore;

namespace Datalake.Inventory.Infrastructure.Database;

/// <summary>
/// Контекст для управления схемой объектов и настроек
/// </summary>
/// Add-Migration NAME -Context Datalake.Inventory.Infrastructure.Database.InventoryDbContext -OutputDir Database\Migrations
/// Remove-Migration -Context Datalake.Data.Infrastructure.Database.DataDbContext
public class InventoryDbContext(DbContextOptions<InventoryDbContext> options) : AbstractDbContext(options)
{
	public override string DefaultSchema { get; } = InventorySchema.Name;

	public override DatabaseTableAccessConfiguration TableAccessConfiguration { get; } = new()
	{
		AccessRules = DatabaseTableAccess.Write,
		Audit = DatabaseTableAccess.Write,
		Blocks = DatabaseTableAccess.Write,
		BlocksProperties = DatabaseTableAccess.Write,
		BlocksTags = DatabaseTableAccess.Write,
		CalculatedAccessRules = DatabaseTableAccess.Write,
		Settings = DatabaseTableAccess.Write,
		Sources = DatabaseTableAccess.Write,
		Tags = DatabaseTableAccess.Write,
		TagsValues = DatabaseTableAccess.Read,
		TagsInputs = DatabaseTableAccess.Write,
		TagsThresholds = DatabaseTableAccess.Write,
		UserGroups = DatabaseTableAccess.Write,
		UserGroupsRelations = DatabaseTableAccess.Write,
		Users = DatabaseTableAccess.Write,
		UserSessions = DatabaseTableAccess.Read,
	};
}
