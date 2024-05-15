using DatalakeDatabase.Enums;
using System.ComponentModel.DataAnnotations;

namespace DatalakeDatabase.ApiModels.Users;

public class UserAuthInfo
{
	[Required]
	public required string UserName { get; set; }

	[Required]
	public required AccessType AccessType { get; set; }

	[Required]
	public required string Token { get; set; }
}
