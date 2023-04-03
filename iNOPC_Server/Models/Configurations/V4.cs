using System.Collections.Generic;

namespace iNOPC.Server.Models.Configurations
{
	public class V4
	{
		public List<Driver> Drivers { get; set; } = new List<Driver>();

		public List<AccessRecord> Access { get; set; } = new List<AccessRecord>();

		public Settings Settings { get; set; } = new Settings();

		public List<Formular> Formulars { get; set; } = new List<Formular>();

		public string Key { get; set; } = "";

		public string Version { get; set; }
	}
}