using System.ComponentModel.DataAnnotations;

namespace Datalake.Database.Models.Sources;

/// <summary>
/// Базовая информация о источнике, достаточная, чтобы на него сослаться
/// </summary>
public class SourceSimpleInfo
{
	/// <summary>
	/// Идентификатор источника в базе данных
	/// </summary>
	[Required]
	public int Id { get; set; }

	/// <summary>
	/// Название источника
	/// </summary>
	[Required]
	public required string Name { get; set; }
}
