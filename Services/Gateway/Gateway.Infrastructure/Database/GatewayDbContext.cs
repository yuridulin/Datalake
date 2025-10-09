using Datalake.Domain.Entities;
using Datalake.Shared.Infrastructure;
using Datalake.Shared.Infrastructure.Schema;
using Microsoft.EntityFrameworkCore;

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
			AccessRules = true,
			Audit = true,
			Blocks = true,
			BlocksProperties = true,
			BlocksTags = true,
			Settings = true,
			Sources = true,
			Tags = true,
			TagsHistory = true,
			TagsInputs = true,
			TagsThresholds = true,
			UserGroups = true,
			UserGroupsRelations = true,
			Users = true,
			UserSessions = false,
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
