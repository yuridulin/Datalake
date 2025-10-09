namespace Datalake.Shared.Api.Constants;

public static class Headers
{
	public static string UserGuidHeader { get; } = "X-Forwarded-User";

	public static string SessionTokenHeander { get; } = "X-Session-Token";
}
