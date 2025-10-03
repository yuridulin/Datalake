using Datalake.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace Datalake.Inventory.Infrastructure.Database;

/// <summary>
/// Контекст базы данных EF
/// </summary>
/// Add-Migration Initial -Context Datalake.Inventory.Infrastructure.Database.InventoryDbContext
public class InventoryDbContext(DbContextOptions<InventoryDbContext> options) : DbContext(options)
{
	/// <summary>
	/// Конфигурация связей между таблицами БД
	/// </summary>
	protected override void OnModelCreating(ModelBuilder modelBuilder)
	{
		modelBuilder.HasDefaultSchema("public");

		// Автоматически применяет все классы конфигурации
		// из текущей сборки, реализующие IEntityTypeConfiguration<>
		modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
	}

	#region Таблицы

	/// <summary>
	/// Таблица прав доступа
	/// </summary>
	public virtual DbSet<AccessRuleEntity> AccessRights { get; set; }

	/// <summary>
	/// Таблица блоков
	/// </summary>
	public virtual DbSet<BlockEntity> Blocks { get; set; }

	/// <summary>
	/// Таблица статичный свойств блоков
	/// </summary>
	public virtual DbSet<BlockPropertyEntity> BlockProperties { get; set; }

	/// <summary>
	/// Таблица связей блоков и тегов
	/// </summary>
	public virtual DbSet<BlockTagEntity> BlockTags { get; set; }

	/// <summary>
	/// Таблица логов аудита
	/// </summary>
	public virtual DbSet<AuditEntity> Audit { get; set; }

	/// <summary>
	/// Таблица настроек
	/// </summary>
	public virtual DbSet<SettingsEntity> Settings { get; set; }

	/// <summary>
	/// Таблица источников
	/// </summary>
	public virtual DbSet<SourceEntity> Sources { get; set; }

	/// <summary>
	/// Таблица тегов
	/// </summary>
	public virtual DbSet<TagEntity> Tags { get; set; }

	/// <summary>
	/// Таблица входных параметров для вычисляемых тегов
	/// </summary>
	public virtual DbSet<TagInputEntity> TagInputs { get; set; }

	/// <summary>
	/// Таблица порогов для пороговых тегов
	/// </summary>
	//public virtual DbSet<TagThresholdEntity> TagThresholds { get; set; }

	/// <summary>
	/// Таблица пользователей
	/// </summary>
	public virtual DbSet<UserEntity> Users { get; set; }

	/// <summary>
	/// Таблица групп пользователей
	/// </summary>
	public virtual DbSet<UserGroupEntity> UserGroups { get; set; }

	/// <summary>
	/// Таблица связей пользователей и групп пользователей
	/// </summary>
	public virtual DbSet<UserGroupRelationEntity> UserGroupRelations { get; set; }

	/// <summary>
	/// Представление пользователей EnergoId с их данными
	/// </summary>
	public virtual DbSet<EnergoIdEntity> EnergoIdView { get; set; }

	#endregion
}
