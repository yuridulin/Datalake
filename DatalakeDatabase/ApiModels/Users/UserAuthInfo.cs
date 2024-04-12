using DatalakeDatabase.Enums;

namespace DatalakeDatabase.ApiModels.Users;

public class UserAuthInfo
{
	public required string UserName { get; set; }

	public AccessType AccessType { get; set; }
}
