namespace Logger_Server.Http.Api
{
	public static class Configuration
	{
		public static object Ping()
		{
			return new { Done = true };
		}

		public static object PingWithBody(Req req)
		{
			return new { Done = true, req.Message };
		}
	}

	public class Req
	{
		public string Message { get; set; }
	}
}
