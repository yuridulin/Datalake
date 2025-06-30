using Datalake.PublicApi.Enums;

namespace Datalake.Database.Extensions;

/// <summary>
/// Расширение работы с уровнями доступа
/// </summary>
public static class AccessTypeExtension
{
	/// <summary>
	/// Проверка, что уровень доступа достаточен по сравнению с необходимым
	/// </summary>
	/// <param name="current">Текущий уровень доступа</param>
	/// <param name="minimal">Минимально необходимый уровень доступа</param>
	/// <returns>Флаг достаточности</returns>
	public static bool HasAccess(this AccessType current, AccessType minimal)
	{
		return current >= minimal;
	}
}
