using DatalakeDatabase.Enums;

namespace DatalakeDatabase.ApiModels.Users;

public class UserLoginPass
{
	public string? Name { get; set; }

	public required string Password { get; set; }

	public AccessType AccessType { get; set; } = AccessType.NOT;
}
