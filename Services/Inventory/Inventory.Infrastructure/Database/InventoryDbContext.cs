using Datalake.Domain.Entities;
using Datalake.Shared.Infrastructure;
using Datalake.Shared.Infrastructure.Configurations;
using Microsoft.EntityFrameworkCore;

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
		modelBuilder.HasDefaultSchema("public");

		modelBuilder.ApplyConfigurations(new()
		{
			AccessRules = false,
			Audit = false,
			Blocks = false,
			BlocksProperties = false,
			BlocksTags = false,
			Settings = false,
			Sources = false,
			Tags = false,
			TagsHistory = true,
			TagsInputs = false,
			TagsThresholds = false,
			UserGroups = false,
			UserGroupsRelations = false,
			Users = false,
		});
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
