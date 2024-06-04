using System.ComponentModel.DataAnnotations;

namespace DatalakeApiClasses.Models.UserGroups;

public class CreateUserGroupRequest
{
	/// <summary>
	/// Название. Не может повторяться в рамках родительской группы
	/// </summary>
	[Required]
	public required string Name { get; set; }

	/// <summary>
	/// Идентификатор родительской группы
	/// </summary>
	public Guid? ParentGroupGuid { get; set; }

	/// <summary>
	/// Описание
	/// </summary>
	public string? Description { get; set; }
}
