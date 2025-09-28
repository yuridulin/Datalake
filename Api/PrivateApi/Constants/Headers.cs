namespace Datalake.PrivateApi.Constants;

public static class Headers
{
	public static string UserHeader => "X-Forwarded-User";

	public static string UnderlyingUserHeader => "X-Forwarded-Underlying-User";
}
