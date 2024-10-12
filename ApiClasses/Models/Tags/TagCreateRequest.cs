using Datalake.ApiClasses.Enums;
using System.ComponentModel.DataAnnotations;

namespace Datalake.ApiClasses.Models.Tags;

/// <summary>
/// Необходимые данные для создания тега
/// </summary>
public class TagCreateRequest
{
	/// <summary>
	/// Наименование тега. Если не указать, будет составлено автоматически
	/// </summary>
	public string? Name { get; set; }

	/// <summary>
	/// Тип значений тега
	/// </summary>
	[Required]
	public required TagType TagType { get; set; }

	/// <summary>
	/// Идентификатор источника данных
	/// </summary>
	public int? SourceId { get; set; }

	/// <summary>
	/// Путь к данным при использовании удалённого источника
	/// </summary>
	public string? SourceItem { get; set; }

	/// <summary>
	/// Идентификатор блока, к которому будет привязан новый тег
	/// </summary>
	public int? BlockId { get; set; }
}
