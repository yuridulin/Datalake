using DatalakeDatabase.Enums;
using System.ComponentModel.DataAnnotations;

namespace DatalakeDatabase.ApiModels.Users;

public class UserInfo
{
	[Required]
	public required string LoginName { get; set; }

	public string? FullName { get; set; }

	[Required]
	public required AccessType AccessType { get; set; }

	[Required]
	public required bool IsStatic { get; set; }
}
