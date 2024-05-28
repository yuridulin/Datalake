using DatalakeApiClasses.Enums;
using System.ComponentModel.DataAnnotations;

namespace DatalakeApiClasses.Models.Users;

public class UserAuthInfo
{
	[Required]
	public required Guid UserGuid { get; set; }

	[Required]
	public required string UserName { get; set; }

	[Required]
	public required string Token { get; set; }

	[Required]
	public required AccessType GlobalAccessType { get; set; }

	[Required]
	public required UserAccessRightsInfo[] Rights { get; set; }
}
