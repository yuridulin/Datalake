using Datalake.Domain.Entities;
using Datalake.Domain.ValueObjects;
using Datalake.Shared.Infrastructure.Database.Configurations.LinqToDb;
using LinqToDB;
using LinqToDB.Data;
using LinqToDB.Mapping;

namespace Datalake.Shared.Infrastructure.Database;

/// <summary>
/// Абстрактный базовый класс для контекстов LinqToDB
/// </summary>
public abstract class AbstractLinqToDbContext(DataOptions options, MappingSchema mappingSchema)
	: DataConnection(options.UseMappingSchema(mappingSchema))
{
	#region Настройка

	/// <summary>
	/// Применить конфигурации к построителю маппинга
	/// </summary>
	protected static void ApplyConfigurations(
		FluentMappingBuilder builder,
		DatabaseTableAccessConfiguration accessConfiguration)
	{
		builder.ApplyConfiguration(new AccessRuleConfiguration(accessConfiguration.AccessRules));
		builder.ApplyConfiguration(new AuditConfiguration(accessConfiguration.Audit));
		builder.ApplyConfiguration(new BlockConfiguration(accessConfiguration.Blocks));
		builder.ApplyConfiguration(new BlockPropertyConfiguration(accessConfiguration.BlocksProperties));
		builder.ApplyConfiguration(new BlockTagConfiguration(accessConfiguration.BlocksTags));
		builder.ApplyConfiguration(new CalculatedAccessRuleConfiguration(accessConfiguration.CalculatedAccessRules));
		builder.ApplyConfiguration(new EnergoIdConfiguration());
		builder.ApplyConfiguration(new SettingsConfiguration(accessConfiguration.Settings));
		builder.ApplyConfiguration(new SourceConfiguration(accessConfiguration.Sources));
		builder.ApplyConfiguration(new TagConfiguration(accessConfiguration.Tags));
		builder.ApplyConfiguration(new TagInputConfiguration(accessConfiguration.TagsInputs));
		builder.ApplyConfiguration(new TagThresholdConfiguration(accessConfiguration.TagsThresholds));
		builder.ApplyConfiguration(new TagValueConfiguration(accessConfiguration.TagsValues));
		builder.ApplyConfiguration(new UserConfiguration(accessConfiguration.Users));
		builder.ApplyConfiguration(new UserGroupConfiguration(accessConfiguration.UserGroups));
		builder.ApplyConfiguration(new UserGroupRelationConfiguration(accessConfiguration.UserGroupsRelations));
		builder.ApplyConfiguration(new UserSessionConfiguration(accessConfiguration.UserSessions));
	}

	#endregion Настройка

	#region Таблицы

	/// <summary>
	/// Таблица прав доступа
	/// </summary>
	public ITable<AccessRule> AccessRules => this.GetTable<AccessRule>();

	/// <summary>
	/// Таблица логов аудита
	/// </summary>
	public ITable<AuditLog> Audit => this.GetTable<AuditLog>();

	/// <summary>
	/// Таблица блоков
	/// </summary>
	public ITable<Block> Blocks => this.GetTable<Block>();

	/// <summary>
	/// Таблица статичных свойств блоков
	/// </summary>
	public ITable<BlockProperty> BlockProperties => this.GetTable<BlockProperty>();

	/// <summary>
	/// Таблица связей блоков и тегов
	/// </summary>
	public ITable<BlockTag> BlockTags => this.GetTable<BlockTag>();

	/// <summary>
	/// Таблица рассчитанных прав доступа
	/// </summary>
	public ITable<CalculatedAccessRule> CalculatedAccessRules => this.GetTable<CalculatedAccessRule>();

	/// <summary>
	/// Представление пользователей EnergoId с их данными
	/// </summary>
	public ITable<EnergoId> EnergoId => this.GetTable<EnergoId>();

	/// <summary>
	/// Таблица настроек
	/// </summary>
	public ITable<Settings> Settings => this.GetTable<Settings>();

	/// <summary>
	/// Таблица источников
	/// </summary>
	public ITable<Source> Sources => this.GetTable<Source>();

	/// <summary>
	/// Таблица тегов
	/// </summary>
	public ITable<Tag> Tags => this.GetTable<Tag>();

	/// <summary>
	/// Таблица входных параметров для вычисляемых тегов
	/// </summary>
	public ITable<TagInput> TagInputs => this.GetTable<TagInput>();

	/// <summary>
	/// Таблица значений тегов
	/// </summary>
	public ITable<TagValue> TagsValues => this.GetTable<TagValue>();

	/// <summary>
	/// Таблица порогов для пороговых тегов
	/// </summary>
	public ITable<TagThreshold> TagThresholds => this.GetTable<TagThreshold>();

	/// <summary>
	/// Таблица пользователей
	/// </summary>
	public ITable<User> Users => this.GetTable<User>();

	/// <summary>
	/// Таблица групп пользователей
	/// </summary>
	public ITable<UserGroup> UserGroups => this.GetTable<UserGroup>();

	/// <summary>
	/// Таблица связей пользователей и групп пользователей
	/// </summary>
	public ITable<UserGroupRelation> UserGroupRelations => this.GetTable<UserGroupRelation>();

	/// <summary>
	/// Таблица текущих сессий пользователей
	/// </summary>
	public ITable<UserSession> UserSessions => this.GetTable<UserSession>();

	#endregion Таблицы
}
