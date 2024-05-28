using System.ComponentModel.DataAnnotations;

namespace DatalakeApiClasses.Models.Users;

public class UserGroupInfo
{
	[Required]
	public required string Guid { get; set; }

	[Required]
	public required string Name { get; set; }
}
