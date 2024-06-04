using DatalakeApiClasses.Enums;

namespace DatalakeApiClasses.Models.UserGroups;

public class UserGroupUsersInfo
{
	public Guid UserGuid { get; set; }

	public AccessType AccessType { get; set; }
}
