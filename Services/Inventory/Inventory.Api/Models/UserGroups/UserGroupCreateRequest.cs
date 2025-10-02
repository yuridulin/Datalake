using System.ComponentModel.DataAnnotations;

namespace Datalake.Inventory.Api.Models.UserGroups;

/// <summary>
/// Данные запроса для создания группы пользователей
/// </summary>
public class UserGroupCreateRequest
{
	/// <summary>
	/// Название. Не может повторяться в рамках родительской группы
	/// </summary>
	[Required]
	public required string Name { get; set; }

	/// <summary>
	/// Идентификатор родительской группы
	/// </summary>
	public Guid? ParentGuid { get; set; }

	/// <summary>
	/// Описание
	/// </summary>
	public string? Description { get; set; }
}
