using DatalakeDatabase.Enums;

namespace DatalakeDatabase.ApiModels.Users;

public class UserUpdateRequest
{
	public required string LoginName { get; set; }

	public string? StaticHost { get; set; }

	public string? Password { get; set; }

	public string? FullName { get; set; }

	public AccessType AccessType { get; set; }

	public bool CreateNewStaticHash { get; set; } = false;
}
