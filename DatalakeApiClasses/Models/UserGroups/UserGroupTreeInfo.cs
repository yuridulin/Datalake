namespace DatalakeApiClasses.Models.UserGroups;

public class UserGroupTreeInfo : UserGroupInfo
{
	public UserGroupTreeInfo[] Children { get; set; } = [];

	public Guid? ParentGuid { get; set; }

	public UserGroupTreeInfo? Parent { get; set; }
}
