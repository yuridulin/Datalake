using Datalake.Shared.Infrastructure.Database;
using Datalake.Shared.Infrastructure.Database.Schema;
using Microsoft.EntityFrameworkCore;

namespace Datalake.Gateway.Infrastructure.Database;

/// <summary>
/// Контекст для работы с сессиями
/// </summary>
/// Add-Migration NAME -Context Datalake.Gateway.Infrastructure.Database.GatewayDbContext -OutputDir Database\Migrations
/// Remove-Migration -Context Datalake.Gateway.Infrastructure.Database.GatewayDbContext
public class GatewayDbContext(DbContextOptions<GatewayDbContext> options) : AbstractDbContext(options)
{
	public override string DefaultSchema { get; } = GatewaySchema.Name;

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
		TagsValues = DatabaseTableAccess.Read,
		TagsInputs = DatabaseTableAccess.Read,
		TagsThresholds = DatabaseTableAccess.Read,
		UserGroups = DatabaseTableAccess.Read,
		UserGroupsRelations = DatabaseTableAccess.Read,
		Users = DatabaseTableAccess.Read,
		UserSessions = DatabaseTableAccess.Write,
	};
}
