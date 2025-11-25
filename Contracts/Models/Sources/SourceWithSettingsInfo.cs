using System.ComponentModel.DataAnnotations;

namespace Datalake.Contracts.Models.Sources;

/// <summary>
/// Информация о источнике
/// </summary>
public class SourceWithSettingsInfo : SourceSimpleInfo
{
	/// <summary>
	/// Используемый для получения данных адрес
	/// </summary>
	public string? Address { get; set; }

	/// <summary>
	/// Источник отмечен как отключенный
	/// </summary>
	[Required]
	public bool IsDisabled { get; set; }
}
