using DatalakeApiClasses.Enums;
using System.ComponentModel.DataAnnotations;

namespace DatalakeApiClasses.Models.Users;

public class UserCreateRequest
{
	[Required]
	public required string LoginName { get; set; }

	public string? FullName { get; set; }

	public string? Password { get; set; }

	public string? StaticHost { get; set; }

	[Required]
	public AccessType AccessType { get; set; }
}
