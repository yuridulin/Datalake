using Datalake.Contracts.Public.Enums;

namespace Datalake.Shared.Domain.ValueObjects;

public class AccessRuleValue(int id, AccessType access)
{
	public int Id { get; private set; } = id;

	public AccessType Access { get; private set; } = access;

	/// <summary>
	/// Проверка, что уровень доступа достаточен по сравнению с необходимым
	/// </summary>
	/// <param name="minimal">Минимально необходимый уровень доступа</param>
	/// <returns>Флаг достаточности</returns>
	public bool HasAccess(AccessType minimal)
	{
		return Access >= minimal;
	}

	public static AccessRuleValue GetDefault() => new(0, AccessType.NotSet);
}
