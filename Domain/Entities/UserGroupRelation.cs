using Datalake.Contracts.Public.Enums;
using Datalake.Domain.Interfaces;

namespace Datalake.Domain.Entities;

/// <summary>
/// Запись в таблице связей учетных записей и групп пользователей
/// </summary>
public record class UserGroupRelation : IWithIdentityKey
{
	private UserGroupRelation() { }

	public UserGroupRelation(Guid userGroupGuid, Guid userGuid, AccessType accessType)
	{
		UserGroupGuid = userGroupGuid;
		UserGuid = userGuid;
		AccessType = accessType;
	}

	// поля в БД

	/// <summary>
	/// Идентификатор
	/// </summary>
	public int Id { get; private set; }

	/// <summary>
	/// Идентификатор учетной записи
	/// </summary>
	public Guid UserGuid { get; private set; }

	/// <summary>
	/// Идентификатор группы пользователей
	/// </summary>
	public Guid UserGroupGuid { get; private set; }

	/// <summary>
	/// Уровень доступа пользователя к группе
	/// </summary>
	public AccessType AccessType { get; private set; }

	// связи

	/// <summary>
	/// Пользователь
	/// </summary>
	public User User { get; set; } = null!;

	/// <summary>
	/// Группа пользователей
	/// </summary>
	public UserGroup UserGroup { get; set; } = null!;
}
