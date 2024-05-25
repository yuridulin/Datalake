using DatalakeDatabase.Enums;
using System.ComponentModel.DataAnnotations;

namespace DatalakeDatabase.ApiModels.Users;

public class UserUpdateRequest
{
	[Required]
	public required string LoginName { get; set; }

	public string? StaticHost { get; set; }

	public string? Password { get; set; }

	public string? FullName { get; set; }

	[Required]
	public AccessType AccessType { get; set; }

	[Required]
	public bool CreateNewStaticHash { get; set; } = false;
}
