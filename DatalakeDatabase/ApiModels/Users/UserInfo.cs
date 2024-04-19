using DatalakeDatabase.Enums;

namespace DatalakeDatabase.ApiModels.Users;

public class UserInfo
{
	public required string LoginName { get; set; }

	public string? FullName { get; set; }

	public required AccessType AccessType { get; set; }

	public required bool IsStatic { get; set; }
}
