using DatalakeDatabase.Enums;
using System.ComponentModel.DataAnnotations;

namespace DatalakeDatabase.ApiModels.Tags;

/// <summary>
/// Необходимые данные для создания тега
/// </summary>
public class TagCreateRequest
{
	/// <summary>
	/// Наименование тега. Если не указать, будет составлено автоматически
	/// </summary>
	public string? Name { get; set; }

	[Required]
	/// <summary>
	/// Тип значений тега
	/// </summary>
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
	/// Идентификатор сущности, к которой будет привязан новый тег
	/// </summary>
	public int? BlockId { get; set; }
}
