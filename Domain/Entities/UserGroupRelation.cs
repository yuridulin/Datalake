using Datalake.Contracts.Public.Enums;
using Datalake.Domain.Interfaces;

namespace Datalake.Domain.Entities;

/// <summary>
/// Запись в таблице связей учетных записей и групп пользователей
/// </summary>
public record class UserGroupRelation : IWithIdentityKey
{
	#region Конструкторы

	private UserGroupRelation() { }

	/// <summary>
	/// Создание новой связи учетной записи с группой
	/// </summary>
	/// <param name="userGroupGuid">Идентификатор группы</param>
	/// <param name="userGuid">Идентификатор учетной записи</param>
	/// <param name="accessType">Уровень доступа в рамках группы</param>
	public UserGroupRelation(Guid userGroupGuid, Guid userGuid, AccessType accessType)
	{
		UserGroupGuid = userGroupGuid;
		UserGuid = userGuid;
		AccessType = accessType;
	}

	#endregion Конструкторы

	#region Свойства

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

	#endregion Свойства

	#region Связи

	/// <summary>
	/// Пользователь
	/// </summary>
	public User User { get; set; } = null!;

	/// <summary>
	/// Группа пользователей
	/// </summary>
	public UserGroup UserGroup { get; set; } = null!;

	#endregion Связи
}
