using System.ComponentModel.DataAnnotations;

namespace DatalakeApiClasses.Models.Users;

public class UserGroupsTreeInfo : UserGroupsInfo
{
	[Required]
	public required UserGroupsTreeInfo[] Children { get; set; }
}
