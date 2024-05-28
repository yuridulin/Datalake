using DatalakeApiClasses.Enums;
using System.ComponentModel.DataAnnotations;

namespace DatalakeApiClasses.Models.Users;

public class UserInfo
{
	[Required]
	public required Guid UserGuid { get; set; }

	[Required]
	public required string LoginName { get; set; }

	public string? FullName { get; set; }

	[Required]
	public required AccessType AccessType { get; set; }

	[Required]
	public required bool IsStatic { get; set; }

	[Required]
	public UserGroupInfo[] UserGroups { get; set; } = [];
}
