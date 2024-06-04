namespace DatalakeApiClasses.Models.UserGroups;

public class UserGroupInfo
{
	public required Guid UserGroupGuid { get; set; }

	public required string Name { get; set; }

	public string? Description { get; set; }
}
