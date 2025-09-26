using Datalake.InventoryService.Database.Views;
using Datalake.PublicApi.Enums;

namespace Datalake.InventoryService.Database.Tables;

/// <summary>
/// Запись в таблице учетных записей
/// </summary>
public record class User
{
	private User() { }

	// поля в БД

	/// <summary>
	/// Идентификатор
	/// </summary>
	public required Guid Guid { get; set; }

	/// <summary>
	/// Тип учетной записи
	/// </summary>
	public UserType Type { get; set; }

	/// <summary>
	/// Полное имя
	/// </summary>
	public string? FullName { get; set; } = string.Empty;

	/// <summary>
	/// Учетная запись отмечена как удаленная
	/// </summary>
	public bool IsDeleted { get; set; } = false;

	// для локальных

	/// <summary>
	/// Имя для входа
	/// </summary>
	public string? Login { get; set; }

	/// <summary>
	/// Хэш пароля
	/// </summary>
	public string? PasswordHash { get; set; }

	// для статичных

	/// <summary>
	/// Адрес, с которого разрешен доступ
	/// </summary>
	public string? StaticHost { get; set; }

	// для EnergoId

	/// <summary>
	/// Идентификатор в EnergoId
	/// </summary>
	public Guid? EnergoIdGuid { get; set; }


	// связи

	/// <summary>
	/// Список связей с группами пользователей
	/// </summary>
	public ICollection<UserGroupRelation> GroupsRelations { get; set; } = [];

	/// <summary>
	/// Список групп пользователей
	/// </summary>
	public ICollection<UserGroup> Groups { get; set; } = [];

	/// <summary>
	/// Список правил доступа, выданных этой учетной записи
	/// </summary>
	public ICollection<AccessRights> AccessRightsList { get; set; } = [];

	/// <summary>
	/// Список действий пользователя, записанных в аудит
	/// </summary>
	public ICollection<Log> Actions { get; set; } = [];

	/// <summary>
	/// Список сообщений аудита
	/// </summary>
	public ICollection<Log> Logs { get; set; } = null!;

	/// <summary>
	/// Информация о пользователе из EnergoId
	/// </summary>
	public EnergoIdUserView? EnergoId { get; set; }

	/// <summary>
	/// Список открытых сессий
	/// </summary>
	public ICollection<UserSession> Sessions { get; set; } = null!;
}
