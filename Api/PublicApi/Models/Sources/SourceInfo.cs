using Datalake.PublicApi.Abstractions;
using Datalake.PublicApi.Enums;
using Datalake.PublicApi.Models.Auth;
using System.ComponentModel.DataAnnotations;

namespace Datalake.PublicApi.Models.Sources;

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
	public required bool IsDisabled { get; set; }

	/// <inheritdoc />
	public AccessRuleInfo AccessRule { get; set; } = AccessRuleInfo.Default;
}
