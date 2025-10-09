using Datalake.Contracts.Public.Enums;
using Datalake.Contracts.Public.Interfaces;
using Datalake.Contracts.Public.Models;
using System.ComponentModel.DataAnnotations;

namespace Datalake.Inventory.Api.Models.Sources;

/// <summary>
/// Информация о источнике
/// </summary>
public class SourceInfo : SourceSimpleInfo, IProtectedEntity
{
	/// <summary>
	/// Произвольное описание источника
	/// </summary>
	public string? Description { get; set; }

	/// <summary>
	/// Используемый для получения данных адрес
	/// </summary>
	public string? Address { get; set; }

	/// <summary>
	/// Тип протокола, по которому запрашиваются данные
	/// </summary>
	[Required]
	public SourceType Type { get; set; }

	/// <summary>
	/// Источник отмечен как удаленный
	/// </summary>
	[Required]
	public bool IsDisabled { get; set; }

	/// <inheritdoc />
	public AccessRuleInfo AccessRule { get; set; } = AccessRuleInfo.Default;
}
