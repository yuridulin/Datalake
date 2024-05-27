using DatalakeApiClasses.Enums;

namespace DatalakeApp.Services.SessionManager.Models;

public class AuthSession
{
	public required string Login { get; set; }

	public required string Token { get; set; }

	public required DateTime ExpirationTime { get; set; }

	public required AccessType AccessType { get; set; }
}
