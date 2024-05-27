using System.ComponentModel.DataAnnotations;

namespace DatalakeApiClasses.Models.Users;

public class UserLoginPass
{
	[Required]
	public required string Name { get; set; }

	[Required]
	public required string Password { get; set; }
}
