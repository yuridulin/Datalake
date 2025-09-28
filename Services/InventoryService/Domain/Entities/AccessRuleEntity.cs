using Datalake.PublicApi.Enums;

namespace Datalake.InventoryService.Domain.Entities;

/// <summary>
/// Запись в таблице правил доступа
/// </summary>
public record class AccessRuleEntity
{
	private AccessRuleEntity() { }

	public AccessRuleEntity(Guid? userGuid, Guid? userGroupGuid, bool isGlobal, int? tagId, int? sourceId, int? blockId, AccessType accessType)
	{
		UserGuid = userGuid;
		UserGroupGuid = userGroupGuid;
		IsGlobal = isGlobal;
		TagId = tagId;
		SourceId = sourceId;
		BlockId = blockId;
		AccessType = accessType;
	}

	// поля в БД

	/// <summary>
	/// Идентификатор
	/// </summary>
	public int Id { get; private set; }

	/// <summary>
	/// Идентификатор пользователя, на которого выдано правило
	/// </summary>
	public Guid? UserGuid { get; private set; }

	/// <summary>
	/// Идентификатор группы, на которую выдано правило
	/// </summary>
	public Guid? UserGroupGuid { get; private set; }

	/// <summary>
	/// Это правило глобальное для всего приложения?
	/// </summary>
	public bool IsGlobal { get; private set; }

	/// <summary>
	/// Идентификатор тега, на который действует правило
	/// </summary>
	public int? TagId { get; private set; }

	/// <summary>
	/// Идентификатор источника, на который действует правило
	/// </summary>
	public int? SourceId { get; private set; }

	/// <summary>
	/// Идентификатор блока, на который действует правило
	/// </summary>
	public int? BlockId { get; private set; }

	/// <summary>
	/// Выданный уровень доступа
	/// </summary>
	public AccessType AccessType { get; private set; } = AccessType.NoAccess;

	// связи

	/// <summary>
	/// Пользователь, на которого выдано правило
	/// </summary>
	public UserEntity? User { get; set; }

	/// <summary>
	/// Группа пользователей, на которую выдано правило
	/// </summary>
	public UserGroupEntity? UserGroup { get; set; }

	/// <summary>
	/// Тег, на который действует правило
	/// </summary>
	public TagEntity? Tag { get; set; }

	/// <summary>
	/// Источник, на который действует правило
	/// </summary>
	public SourceEntity? Source { get; set; }

	/// <summary>
	/// Блок, на который действует правило
	/// </summary>
	public BlockEntity? Block { get; set; }

	/// <summary>
	/// Список сообщений аудита
	/// </summary>
	public ICollection<AuditEntity> Logs { get; set; } = null!;
}
