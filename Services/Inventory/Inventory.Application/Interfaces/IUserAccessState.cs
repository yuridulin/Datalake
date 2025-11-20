using Datalake.Domain.ValueObjects;

namespace Datalake.Inventory.Application.Interfaces;

/// <summary>
/// Снимок текущих вычисленных прав доступа
/// </summary>
public interface IUserAccessState
{
	/// <summary>
	/// Версия снимка
	/// </summary>
	long Version { get; }

	/// <summary>
	/// Вычисленные права доступа для учетных записей
	/// </summary>
	IReadOnlyDictionary<Guid, UserAccessValue> UsersAccess { get; }
}