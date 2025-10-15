using Datalake.Contracts.Public.Enums;

namespace Datalake.Domain.ValueObjects;

/// <summary>
/// Рассчитанный уровень доступа
/// </summary>
public record struct UserAccessRuleValue
{
	/// <summary>
	/// Создание рассчитанного уровня доступа
	/// </summary>
	/// <param name="id">Идентификатор правила</param>
	/// <param name="access">Уровень доступа</param>
	public UserAccessRuleValue(int id, AccessType access)
	{
		Id = id;
		Access = access;
	}

	/// <summary>
	/// Идентификатор правила, на основе которого выполнен расчет
	/// </summary>
	public int Id { get; private set; }

	/// <summary>
	/// Уровень доступа
	/// </summary>
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

	/// <summary>
	/// Правило по умолчанию
	/// </summary>
	public static UserAccessRuleValue Empty => new(0, AccessType.None);
}
