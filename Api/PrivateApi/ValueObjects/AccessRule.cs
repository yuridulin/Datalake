using Datalake.PublicApi.Enums;

namespace Datalake.PrivateApi.ValueObjects;

public class AccessRule
{
	public int Id { get; private set; }

	public AccessType Access { get; private set; }

	/// <summary>
	/// Проверка, что уровень доступа достаточен по сравнению с необходимым
	/// </summary>
	/// <param name="minimal">Минимально необходимый уровень доступа</param>
	/// <returns>Флаг достаточности</returns>
	public bool HasAccess(AccessType minimal)
	{
		return Access >= minimal;
	}
}
