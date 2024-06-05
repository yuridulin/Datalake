using System.ComponentModel.DataAnnotations;

namespace DatalakeApiClasses.Models.UserGroups;

public class UserGroupTreeInfo : UserGroupInfo
{
	[Required]
	public UserGroupTreeInfo[] Children { get; set; } = [];

	public Guid? ParentGuid { get; set; }

	public UserGroupTreeInfo? Parent { get; set; }
}
