using System.ComponentModel.DataAnnotations;

namespace DatalakeApiClasses.Models.Users;

public class UserDetailInfo : UserInfo
{
	[Required]
	public required string Hash { get; set; }

	public string? StaticHost { get; set; }
}
