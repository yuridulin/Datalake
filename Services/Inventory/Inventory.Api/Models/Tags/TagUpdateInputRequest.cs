using System.ComponentModel.DataAnnotations;

namespace Datalake.Inventory.Api.Models.Tags;

/// <summary>
/// Необходимая информация для привязки тега в качестве входного для 
/// </summary>
public class TagUpdateInputRequest
{
	/// <summary>
	/// Название переменной
	/// </summary>
	[Required]
	public required string VariableName { get; set; }

	/// <summary>
	/// Идентификатор закрепленного тега
	/// </summary>
	[Required]
	public required int TagId { get; set; }

	/// <summary>
	/// Идентификатор связи, по которой выбран закрепленный тег
	/// </summary>
	[Required]
	public required int BlockId { get; set; }
}
