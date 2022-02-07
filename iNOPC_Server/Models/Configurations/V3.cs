using System.Collections.Generic;

namespace iNOPC.Server.Models.Configurations
{
	public class V3
	{
		public List<Driver> Drivers { get; set; } = new List<Driver>();

		public List<AccessRecord> Access { get; set; } = new List<AccessRecord>();

		public Settings Settings { get; set; } = new Settings();

		public DatabaseSettings Database { get; set; } = new DatabaseSettings();

		public string Key { get; set; } = "";

		public string Version { get; set; }
	}
}