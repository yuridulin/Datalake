namespace Datalake.Shared.Infrastructure.Schema;

public static class GatewaySchema
{
	public static string Name { get; } = "gateway";

	public static class UserSessions
	{
		public static string Name { get; } = nameof(UserSessions);

		public static class Columns
		{
			public static string Token { get; } = nameof(Token);
		}
	}
}
