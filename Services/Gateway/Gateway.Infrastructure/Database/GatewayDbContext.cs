using Datalake.Domain.Entities;
using Datalake.Shared.Infrastructure;
using Datalake.Shared.Infrastructure.Schema;
using Microsoft.EntityFrameworkCore;
using static Datalake.Shared.Infrastructure.ConfigurationsApplyHelper;

namespace Datalake.Gateway.Infrastructure.Database;

/// <summary>
/// Контекст для работы с сессиями
/// </summary>
/// Add-Migration NAME -Context Datalake.Gateway.Infrastructure.Database.GatewayDbContext -OutputDir Database\Migrations
/// Remove-Migration -Context Datalake.Gateway.Infrastructure.Database.GatewayDbContext
public class GatewayDbContext(DbContextOptions<GatewayDbContext> options) : DbContext(options)
{
	/// <summary>
	/// Конфигурация связей между таблицами БД
	/// </summary>
	protected override void OnModelCreating(ModelBuilder modelBuilder)
	{
		modelBuilder.HasDefaultSchema(GatewaySchema.Name);

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
			TagsValues = TableAccess.Read,
			TagsInputs = TableAccess.Read,
			TagsThresholds = TableAccess.Read,
			UserGroups = TableAccess.Read,
			UserGroupsRelations = TableAccess.Read,
			Users = TableAccess.Read,
			UserSessions = TableAccess.Write,
		});
	}

	/// <summary>
	/// Таблица пользователей
	/// </summary>
	public virtual DbSet<User> Users { get; set; }

	/// <summary>
	/// Таблица текущих сессий пользователей
	/// </summary>
	public virtual DbSet<UserSession> UserSessions { get; set; }
}
