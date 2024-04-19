using DatalakeDatabase.Enums;
using System.Security.Cryptography;
using System.Text;

namespace DatalakeDatabase.ApiModels.Users;

public class UserAuthRequest
{
	public required string LoginName { get; set; }

	public string? FullName { get; set; }

	public string? Password { get; set; }

	public string? StaticHost { get; set; }

	public AccessType AccessType { get; set; }
}
