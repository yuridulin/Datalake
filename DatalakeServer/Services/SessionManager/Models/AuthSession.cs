using DatalakeApiClasses.Models.Users;

namespace DatalakeServer.Services.SessionManager.Models;

public class AuthSession
{
	public required UserAuthInfo User { get; set; }

	public required DateTime ExpirationTime { get; set; }
}
