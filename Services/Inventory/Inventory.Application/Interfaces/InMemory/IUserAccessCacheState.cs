using Datalake.Shared.Application.Entities;

namespace Datalake.Inventory.Application.Interfaces.InMemory;

/// <summary>
/// Снимок текущих вычисленных прав доступа
/// </summary>
public interface IUserAccessCacheState
{
	/// <summary>
	/// Версия снимка
	/// </summary>
	long Version { get; }

	/// <summary>
	/// Вычисленные права доступа для учетных записей
	/// </summary>
	IReadOnlyDictionary<Guid, UserAccessEntity> UsersAccess { get; }
}