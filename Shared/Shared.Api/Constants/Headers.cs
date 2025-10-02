namespace Datalake.Shared.Api.Constants;

public static class Headers
{
	public static string UserHeader => "X-Forwarded-User";

	public static string UnderlyingUserHeader => "X-Forwarded-Underlying-User";
}
