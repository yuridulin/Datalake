using Datalake.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace Datalake.Contracts.Models.Sources;

/// <summary>
/// Данные для изменения источника данных
/// </summary>
public class SourceUpdateRequest
{
	/// <summary>
	/// Название источника
	/// </summary>
	[Required]
	public required string Name { get; set; }

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
}
