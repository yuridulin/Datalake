using System.ComponentModel.DataAnnotations;

namespace DatalakeApiClasses.Models.UserGroups;

public class UserGroupDetailedInfo : UserGroupInfo
{
	[Required]
	public required UserGroupUsersInfo[] Users { get; set; } = [];

	[Required]
	public required UserGroupInfo[] Subgroups { get; set; } = [];
}
