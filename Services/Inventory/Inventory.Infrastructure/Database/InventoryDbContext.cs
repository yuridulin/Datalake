using Datalake.Domain.Entities;
using Datalake.Inventory.Infrastructure.Database.Configurations;
using Microsoft.EntityFrameworkCore;

namespace Datalake.Inventory.Infrastructure.Database;

/// <summary>
/// Контекст базы данных EF
/// </summary>
/// Add-Migration SeparateContexts -Context Datalake.Inventory.Infrastructure.Database.InventoryDbContext -OutputDir Database\Migrations
public class InventoryDbContext(DbContextOptions<InventoryDbContext> options) : DbContext(options)
{
	/// <summary>
	/// Конфигурация связей между таблицами БД
	/// </summary>
	protected override void OnModelCreating(ModelBuilder modelBuilder)
	{
		modelBuilder.HasDefaultSchema("public");

		modelBuilder.ApplyConfiguration(new AccessRuleConfiguration());
		modelBuilder.ApplyConfiguration(new AuditConfiguration());
		modelBuilder.ApplyConfiguration(new BlockConfiguration());
		modelBuilder.ApplyConfiguration(new BlockPropertyConfiguration());
		modelBuilder.ApplyConfiguration(new BlockTagConfiguration());
		modelBuilder.ApplyConfiguration(new EnergoIdConfiguration());
		modelBuilder.ApplyConfiguration(new SettingsConfiguration());
		modelBuilder.ApplyConfiguration(new SourceConfiguration());
		modelBuilder.ApplyConfiguration(new TagConfiguration());
		modelBuilder.ApplyConfiguration(new TagInputConfiguration());
		modelBuilder.ApplyConfiguration(new TagThresholdConfiguration());
		modelBuilder.ApplyConfiguration(new UserConfiguration());
		modelBuilder.ApplyConfiguration(new UserGroupConfiguration());
		modelBuilder.ApplyConfiguration(new UserGroupRelationConfiguration());
	}

	#region Таблицы

	/// <summary>
	/// Таблица прав доступа
	/// </summary>
	public virtual DbSet<AccessRights> AccessRights { get; set; }

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
	/// Таблица логов аудита
	/// </summary>
	public virtual DbSet<Log> Audit { get; set; }

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
