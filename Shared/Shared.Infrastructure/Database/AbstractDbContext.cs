using Datalake.Domain.Entities;
using Datalake.Domain.ValueObjects;
using Datalake.Shared.Infrastructure.Database.Configurations.EntityFramework;
using Microsoft.EntityFrameworkCore;

namespace Datalake.Shared.Infrastructure.Database;

public abstract class AbstractDbContext(DbContextOptions options) : DbContext(options)
{
	#region Настройка

	public abstract DatabaseTableAccessConfiguration TableAccessConfiguration { get; }

	public abstract string DefaultSchema { get; }

	protected override void OnModelCreating(ModelBuilder modelBuilder)
	{
		modelBuilder.HasDefaultSchema(DefaultSchema);
		ApplyConfigurations(modelBuilder, TableAccessConfiguration);
	}

	private static void ApplyConfigurations(ModelBuilder modelBuilder, DatabaseTableAccessConfiguration accessConfiguration)
	{
		modelBuilder.ApplyConfiguration(new AccessRuleConfiguration(accessConfiguration.AccessRules));
		modelBuilder.ApplyConfiguration(new AuditConfiguration(accessConfiguration.Audit));
		modelBuilder.ApplyConfiguration(new BlockConfiguration(accessConfiguration.Blocks));
		modelBuilder.ApplyConfiguration(new BlockPropertyConfiguration(accessConfiguration.BlocksProperties));
		modelBuilder.ApplyConfiguration(new BlockTagConfiguration(accessConfiguration.BlocksTags));
		modelBuilder.ApplyConfiguration(new CalculatedAccessRuleConfiguration(accessConfiguration.CalculatedAccessRules));
		modelBuilder.ApplyConfiguration(new EnergoIdConfiguration());
		modelBuilder.ApplyConfiguration(new SettingsConfiguration(accessConfiguration.Settings));
		modelBuilder.ApplyConfiguration(new SourceConfiguration(accessConfiguration.Sources));
		modelBuilder.ApplyConfiguration(new TagConfiguration(accessConfiguration.Tags));
		modelBuilder.ApplyConfiguration(new TagInputConfiguration(accessConfiguration.TagsInputs));
		modelBuilder.ApplyConfiguration(new TagThresholdConfiguration(accessConfiguration.TagsThresholds));
		modelBuilder.ApplyConfiguration(new TagValueConfiguration(accessConfiguration.TagsValues));
		modelBuilder.ApplyConfiguration(new UserConfiguration(accessConfiguration.Users));
		modelBuilder.ApplyConfiguration(new UserGroupConfiguration(accessConfiguration.UserGroups));
		modelBuilder.ApplyConfiguration(new UserGroupRelationConfiguration(accessConfiguration.UserGroupsRelations));
		modelBuilder.ApplyConfiguration(new UserSessionConfiguration(accessConfiguration.UserSessions));
	}

	#endregion Настройка

	#region Таблицы

	/// <summary>
	/// Таблица прав доступа
	/// </summary>
	public virtual DbSet<AccessRule> AccessRules { get; set; }

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
	/// Представление пользователей EnergoId с их данными
	/// </summary>
	public virtual DbSet<EnergoId> EnergoId { get; set; }

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
	/// Таблица значений тегов
	/// </summary>
	public virtual DbSet<TagValue> TagsValues { get; set; }

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
	/// Таблица текущих сессий пользователей
	/// </summary>
	public virtual DbSet<UserSession> UserSessions { get; set; }

	#endregion Таблицы
}
