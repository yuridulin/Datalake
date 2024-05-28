using System.ComponentModel.DataAnnotations;

namespace DatalakeApiClasses.Models.Users;

public class UserGroupTreeInfo : UserGroupInfo
{
	[Required]
	public required UserGroupTreeInfo[] Children { get; set; }
}
