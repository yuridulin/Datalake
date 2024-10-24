using System.ComponentModel.DataAnnotations;

namespace Datalake.Database.Models.Users;

/// <summary>
/// Данные с сервиса "EnergoID"
/// </summary>
public class EnergoIdInfo
{
	/// <summary>
	/// Список пользователей
	/// </summary>
	[Required]
	public UserEnergoIdInfo[] EnergoIdUsers { get; set; } = [];

	/// <summary>
	/// Есть ли связь с сервисом "EnergoID"
	/// </summary>
	[Required]
	public bool Connected { get; set; }
}
