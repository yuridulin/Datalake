using System.Collections.Generic;

namespace iNOPC.Server.Models.Configurations
{
	public class V1
	{
		public List<Driver> Drivers { get; set; } = new List<Driver>();

		public List<AccessRecord> Access { get; set; } = new List<AccessRecord>();

		public string Version { get; set; }
	}
}