using DatalakeApiClasses.Enums;
using System.ComponentModel.DataAnnotations;

namespace DatalakeApiClasses.Models.Users;

public class UserAccessRightsInfo
{
	public required bool IsGlobal { get; set; }

	public int? TagId { get; set; }

	public int? SourceId { get; set; }

	public int? BlockId { get; set; }

	[Required]
	public required AccessType AccessType { get; set; } = AccessType.NoAccess;
}
