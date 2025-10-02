using Datalake.Contracts.Public.Enums;
using System.ComponentModel.DataAnnotations;

namespace Datalake.Inventory.Api.Models.AccessRules;

/// <summary>
/// Общая информация о разрешении
/// </summary>
public class AccessRightsSimpleInfo
{
	/// <summary>
	/// Идентификатор разрешения
	/// </summary>
	[Required]
	public int Id { get; set; }

	/// <summary>
	/// Тип доступа
	/// </summary>
	[Required]
	public AccessType AccessType { get; set; }

	/// <summary>
	/// Является ли разрешение глобальным
	/// </summary>
	[Required]
	public required bool IsGlobal { get; set; }
}
