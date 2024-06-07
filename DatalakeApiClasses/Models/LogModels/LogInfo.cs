using DatalakeApiClasses.Enums;
using System.ComponentModel.DataAnnotations;

namespace DatalakeApiClasses.Models.Logs;

/// <summary>
/// Запись собщения
/// </summary>
public class LogInfo
{
	/// <summary>
	/// Идентификатор записи
	/// </summary>
	[Required]
	public required long Id { get; set; }

	/// <summary>
	/// Дата формата DateFormats.Long
	/// </summary>
	[Required]
	public required string DateString { get; set; }

	/// <summary>
	/// Категория сообщения (к какому объекту относится)
	/// </summary>
	[Required]
	public required LogCategory Category { get; set; }

	/// <summary>
	/// Степень важности сообщения
	/// </summary>
	[Required]
	public required LogType Type { get; set; }

	/// <summary>
	/// Текст сообщеня
	/// </summary>
	[Required]
	public required string Text { get; set; }

	/// <summary>
	/// Ссылка на конкретный объект в случае, если это подразумевает категория
	/// </summary>
	public string? RefId { get; set; }
}
