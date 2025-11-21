using Datalake.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace Datalake.Contracts.Models.Users;

/// <summary>
/// Базовая информация о учетной записи
/// </summary>
public class UserSimpleInfo
{
	/// <summary>
	/// Идентификатор учетной записи
	/// </summary>
	[Required]
	public required Guid Guid { get; set; }

	/// <summary>
	/// Тип учетной записи
	/// </summary>
	[Required]
	public required UserType Type { get; set; }

	/// <summary>
	/// Имя учетной записи
	/// </summary>
	[Required]
	public required string FullName { get; set; }
}
