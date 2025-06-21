using Datalake.PublicApi.Enums;

namespace Datalake.Database.Interfaces;

/// <summary>
/// Модель пользователя, защищенная от записи
/// </summary>
public interface IReadOnlyUser
{
	/// <summary>
	/// Идентификатор
	/// </summary>
	Guid Guid { get; }

	/// <summary>
	/// Тип учетной записи
	/// </summary>
	UserType Type { get; }

	/// <summary>
	/// Полное имя
	/// </summary>
	string? FullName { get; }

	/// <summary>
	/// Учетная запись отмечена как удаленная
	/// </summary>
	bool IsDeleted { get; }

	/// <summary>
	/// Имя для входа
	/// </summary>
	string? Login { get; }

	/// <summary>
	/// Хэш пароля
	/// </summary>
	string? PasswordHash { get; }

	/// <summary>
	/// Адрес, с которого разрешен доступ
	/// </summary>
	string? StaticHost { get; }

	/// <summary>
	/// Идентификатор в EnergoId
	/// </summary>
	Guid? EnergoIdGuid { get; }
} 