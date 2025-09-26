using Datalake.PublicApi.Enums;

namespace Datalake.InventoryService.Database.Tables;

/// <summary>
/// Запись в таблице правил доступа
/// </summary>
public record class AccessRights
{
	private AccessRights() { }

	public AccessRights(Guid? userGuid, Guid? userGroupGuid, bool isGlobal, int? tagId, int? sourceId, int? blockId, AccessType accessType)
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
	public int Id { get; set; }

	/// <summary>
	/// Идентификатор пользователя, на которого выдано правило
	/// </summary>
	public Guid? UserGuid { get; set; }

	/// <summary>
	/// Идентификатор группы, на которую выдано правило
	/// </summary>
	public Guid? UserGroupGuid { get; set; }

	/// <summary>
	/// Это правило глобальное для всего приложения?
	/// </summary>
	public bool IsGlobal { get; set; }

	/// <summary>
	/// Идентификатор тега, на который действует правило
	/// </summary>
	public int? TagId { get; set; }

	/// <summary>
	/// Идентификатор источника, на который действует правило
	/// </summary>
	public int? SourceId { get; set; }

	/// <summary>
	/// Идентификатор блока, на который действует правило
	/// </summary>
	public int? BlockId { get; set; }

	/// <summary>
	/// Выданный уровень доступа
	/// </summary>
	public AccessType AccessType { get; set; } = AccessType.NoAccess;

	// связи

	/// <summary>
	/// Пользователь, на которого выдано правило
	/// </summary>
	public User? User { get; set; }

	/// <summary>
	/// Группа пользователей, на которую выдано правило
	/// </summary>
	public UserGroup? UserGroup { get; set; }

	/// <summary>
	/// Тег, на который действует правило
	/// </summary>
	public Tag? Tag { get; set; }

	/// <summary>
	/// Источник, на который действует правило
	/// </summary>
	public Source? Source { get; set; }

	/// <summary>
	/// Блок, на который действует правило
	/// </summary>
	public Block? Block { get; set; }

	/// <summary>
	/// Список сообщений аудита
	/// </summary>
	public ICollection<Log> Logs { get; set; } = null!;
}
