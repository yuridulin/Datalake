using Datalake.InventoryService.Infrastructure.Database.Views;
using Datalake.PublicApi.Enums;

namespace Datalake.InventoryService.Domain.Entities;

/// <summary>
/// Запись в таблице учетных записей
/// </summary>
public record class UserEntity
{
	private UserEntity() { }

	// поля в БД

	/// <summary>
	/// Идентификатор
	/// </summary>
	public Guid Guid { get; private set; }

	/// <summary>
	/// Тип учетной записи
	/// </summary>
	public UserType Type { get; private set; }

	/// <summary>
	/// Полное имя
	/// </summary>
	public string? FullName { get; private set; } = string.Empty;

	/// <summary>
	/// Учетная запись отмечена как удаленная
	/// </summary>
	public bool IsDeleted { get; private set; } = false;

	// для локальных

	/// <summary>
	/// Имя для входа
	/// </summary>
	public string? Login { get; private set; }

	/// <summary>
	/// Хэш пароля
	/// </summary>
	public string? PasswordHash { get; private set; }

	// для статичных

	/// <summary>
	/// Адрес, с которого разрешен доступ
	/// </summary>
	public string? StaticHost { get; private set; }

	// для EnergoId

	/// <summary>
	/// Идентификатор в EnergoId
	/// </summary>
	public Guid? EnergoIdGuid { get; private set; }


	// связи

	/// <summary>
	/// Список связей с группами пользователей
	/// </summary>
	public ICollection<UserGroupRelationEntity> GroupsRelations { get; set; } = [];

	/// <summary>
	/// Список групп пользователей
	/// </summary>
	public ICollection<UserGroupEntity> Groups { get; set; } = [];

	/// <summary>
	/// Список правил доступа, выданных этой учетной записи
	/// </summary>
	public ICollection<AccessRuleEntity> AccessRightsList { get; set; } = [];

	/// <summary>
	/// Список действий пользователя, записанных в аудит
	/// </summary>
	public ICollection<AuditEntity> Actions { get; set; } = [];

	/// <summary>
	/// Список сообщений аудита
	/// </summary>
	public ICollection<AuditEntity> Logs { get; set; } = null!;

	/// <summary>
	/// Информация о пользователе из EnergoId
	/// </summary>
	public EnergoIdUserView? EnergoId { get; set; }
}
