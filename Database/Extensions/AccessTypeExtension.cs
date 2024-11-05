using Datalake.Database.Enums;

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
		return minimal switch
		{
			AccessType.NotSet => true,

			AccessType.NoAccess =>
				current != AccessType.NoAccess,

			AccessType.Viewer => 
				current == AccessType.Viewer 
				|| current == AccessType.Editor
				|| current == AccessType.Manager
				|| current == AccessType.Admin,

			AccessType.Editor => 
				current == AccessType.Editor
				|| current == AccessType.Manager
				|| current == AccessType.Admin,

			AccessType.Manager =>
				current == AccessType.Manager
				|| current == AccessType.Admin,

			AccessType.Admin =>
				current == AccessType.Admin,

			_ => false,
		};
	}
}
