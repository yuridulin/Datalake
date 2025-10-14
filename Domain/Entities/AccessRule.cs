using Datalake.Contracts.Public.Enums;
using Datalake.Domain.Exceptions;
using Datalake.Domain.Interfaces;

namespace Datalake.Domain.Entities;

/// <summary>
/// Запись в таблице правил доступа
/// </summary>
public record class AccessRule : IWithIdentityKey
{
	private AccessRule() { }

	/// <summary>
	/// Создание нового правила доступа
	/// </summary>
	/// <param name="accessType">Уровень доступа</param>
	/// <param name="userGuid">Идентификатор учетной записи</param>
	/// <param name="userGroupGuid">Идентификатор группы учетных записей</param>
	/// <param name="tagId">Идентификатор тега</param>
	/// <param name="sourceId">Идентификатор источника данных</param>
	/// <param name="blockId">Идентификатор блока</param>
	/// <exception cref="DomainException">Недостаточно данных</exception>
	public AccessRule(
		AccessType accessType,
		Guid? userGuid = null,
		Guid? userGroupGuid = null,
		int? tagId = null,
		int? sourceId = null,
		int? blockId = null)
	{
		AccessType = accessType;

		if (userGuid.HasValue)
		{
			UserGuid = userGuid.Value;
			UserGroupGuid = null;
		}
		else if (userGroupGuid.HasValue)
		{
			UserGuid = null;
			UserGroupGuid = userGroupGuid.Value;
		}
		else
		{
			throw new DomainException("Нужно указать или учетную запись, или группу учетных записей");
		}

		if (tagId.HasValue)
		{
			TagId = tagId.Value;
			BlockId = null;
			SourceId = null;
			IsGlobal = false;
		}
		else if (sourceId.HasValue)
		{
			TagId = null;
			BlockId = null;
			SourceId = sourceId.Value;
			IsGlobal = false;
		}
		else if (blockId.HasValue)
		{
			TagId = null;
			BlockId = blockId.Value;
			SourceId = null;
			IsGlobal = false;
		}
		else
		{
			TagId = null;
			BlockId = null;
			SourceId = null;
			IsGlobal = true;
		}
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
	public AccessType AccessType { get; private set; } = AccessType.Denied;

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
}
