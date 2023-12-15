using System.Collections.Generic;

namespace Datalake.Models
{
	public class StartupOptions
	{
		public int WebServerPort { get; set; } = 83;

		public Dictionary<string, StartupConnection> ConnectionStrings { get; set; } = new Dictionary<string, StartupConnection>();
	}

	public class StartupConnection
	{
		public string Provider { get; set; } = string.Empty;

		public string ConnectionString { get; set; } = string.Empty;
	}
}
