using DatalakeApiClasses.Enums;

namespace DatalakeApiClasses.Models.Users;

public class UserAccessGroupInfo
{
	public required Guid GroupGuid { get; set; }

	public required AccessType AccessType { get; set; }
}
