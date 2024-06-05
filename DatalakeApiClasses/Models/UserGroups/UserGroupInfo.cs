using System.ComponentModel.DataAnnotations;

namespace DatalakeApiClasses.Models.UserGroups;

public class UserGroupInfo
{
	[Required]
	public required Guid UserGroupGuid { get; set; }

	[Required]
	public required string Name { get; set; }

	public string? Description { get; set; }

	public Guid? ParentGroupGuid { get; set; }
}
