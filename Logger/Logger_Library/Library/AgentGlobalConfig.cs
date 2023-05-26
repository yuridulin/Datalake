namespace Logger.Library
{
	public class AgentGlobalConfig
	{
		public string Server { get; set; }

		public int Port { get; set; } = 4330;

		public int ReplyIntervalSeconds { get; set; } = 10;
	}
}
