using Datalake.ApiClasses.Enums;

namespace Datalake.ApiClasses.Constants;

/// <summary>
/// Комбинации значений, используемые для проверок
/// </summary>
public static class EnumSet
{
	/// <summary>
	/// Вариант доступа, с которым пользователь может обращаться к вложенным группам
	/// </summary>
	public static AccessType[] UserWithAccess { get; set; } = [AccessType.User, AccessType.Admin];
}
