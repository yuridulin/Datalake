using Datalake.Domain.Entities;
using Datalake.Domain.ValueObjects;
using Datalake.Shared.Infrastructure;
using Datalake.Shared.Infrastructure.Schema;
using Microsoft.EntityFrameworkCore;
using static Datalake.Shared.Infrastructure.ConfigurationsApplyHelper;

namespace Datalake.Inventory.Infrastructure.Database;

/// <summary>
/// Контекст для управления схемой объектов и настроек
/// </summary>
/// Add-Migration NAME -Context Datalake.Inventory.Infrastructure.Database.InventoryDbContext -OutputDir Database\Migrations
/// Remove-Migration -Context Datalake.Data.Infrastructure.Database.DataDbContext
public class InventoryDbContext(DbContextOptions<InventoryDbContext> options) : DbContext(options)
{
	/// <summary>
	/// Конфигурация связей между таблицами БД
	/// </summary>
	protected override void OnModelCreating(ModelBuilder modelBuilder)
	{
		modelBuilder.HasDefaultSchema(InventorySchema.Name);

		modelBuilder.ApplyConfigurations(new()
		{
			AccessRules = TableAccess.Write,
			Audit = TableAccess.Write,
			Blocks = TableAccess.Write,
			BlocksProperties = TableAccess.Write,
			BlocksTags = TableAccess.Write,
			CalculatedAccessRules = TableAccess.Write,
			Settings = TableAccess.Write,
			Sources = TableAccess.Write,
			Tags = TableAccess.Write,
			TagsHistory = TableAccess.Read,
			TagsInputs = TableAccess.Write,
			TagsThresholds = TableAccess.Write,
			UserGroups = TableAccess.Write,
			UserGroupsRelations = TableAccess.Write,
			Users = TableAccess.Write,
			UserSessions = TableAccess.Read,
		});
	}

	#region Таблицы

	/// <summary>
	/// Таблица прав доступа
	/// </summary>
	public virtual DbSet<AccessRule> AccessRights { get; set; }

	/// <summary>
	/// Таблица логов аудита
	/// </summary>
	public virtual DbSet<AuditLog> Audit { get; set; }

	/// <summary>
	/// Таблица блоков
	/// </summary>
	public virtual DbSet<Block> Blocks { get; set; }

	/// <summary>
	/// Таблица статичный свойств блоков
	/// </summary>
	public virtual DbSet<BlockProperty> BlockProperties { get; set; }

	/// <summary>
	/// Таблица связей блоков и тегов
	/// </summary>
	public virtual DbSet<BlockTag> BlockTags { get; set; }

	/// <summary>
	/// Таблица рассчитанных прав доступа
	/// </summary>
	public virtual DbSet<CalculatedAccessRule> CalculatedAccessRules { get; set; }

	/// <summary>
	/// Таблица настроек
	/// </summary>
	public virtual DbSet<Settings> Settings { get; set; }

	/// <summary>
	/// Таблица источников
	/// </summary>
	public virtual DbSet<Source> Sources { get; set; }

	/// <summary>
	/// Таблица тегов
	/// </summary>
	public virtual DbSet<Tag> Tags { get; set; }

	/// <summary>
	/// Таблица входных параметров для вычисляемых тегов
	/// </summary>
	public virtual DbSet<TagInput> TagInputs { get; set; }

	/// <summary>
	/// Таблица порогов для пороговых тегов
	/// </summary>
	public virtual DbSet<TagThreshold> TagThresholds { get; set; }

	/// <summary>
	/// Таблица пользователей
	/// </summary>
	public virtual DbSet<User> Users { get; set; }

	/// <summary>
	/// Таблица групп пользователей
	/// </summary>
	public virtual DbSet<UserGroup> UserGroups { get; set; }

	/// <summary>
	/// Таблица связей пользователей и групп пользователей
	/// </summary>
	public virtual DbSet<UserGroupRelation> UserGroupRelations { get; set; }

	/// <summary>
	/// Представление пользователей EnergoId с их данными
	/// </summary>
	public virtual DbSet<EnergoId> EnergoIdView { get; set; }

	#endregion
}
