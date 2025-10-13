using Datalake.Contracts.Public.Enums;

namespace Datalake.Shared.Application.Entities;

public record class AccessRuleValue
{
	public AccessRuleValue(int id, AccessType access)
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
	public bool HasAccess(AccessType minimal)
	{
		return Access >= minimal;
	}

	public static AccessRuleValue GetDefault() => new(0, AccessType.None);
}
