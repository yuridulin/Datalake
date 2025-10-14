using Datalake.Contracts.Public.Enums;

namespace Datalake.Domain.ValueObjects;

public record struct UserAccessRuleValue
{
	public UserAccessRuleValue(int id, AccessType access)
	{
		Id = id;
		Access = access;
	}

	public int Id { get; private set; }

	public AccessType Access { get; private set; }

	/// <summary>
	/// Проверка, что уровень доступа достаточен по сравнению с необходимым
	/// </summary>
	/// <param name="minimal">Минимально необходимый уровень доступа</param>
	/// <returns>Флаг достаточности</returns>
	public readonly bool HasAccess(AccessType minimal)
	{
		return Access >= minimal;
	}

	public static UserAccessRuleValue GetDefault() => new(0, AccessType.None);
}
