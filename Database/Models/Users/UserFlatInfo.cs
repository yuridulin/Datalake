using Datalake.Database.Enums;
using System.ComponentModel.DataAnnotations;

namespace Datalake.Database.Models.Users;

/// <summary>
/// Сокращенная информация о пользователе без каких-либо связей
/// </summary>
public class UserFlatInfo : UserSimpleInfo
{
	/// <summary>
	/// Имя для входа
	/// </summary>
	public string? Login { get; set; }

	/// <summary>
	/// Тип учётной записи
	/// </summary>
	[Required]
	public UserType Type { get; set; }

	/// <summary>
	/// Идентификатор пользователя в сервере EnergoId
	/// </summary>
	public Guid? EnergoIdGuid { get; set; }
}
