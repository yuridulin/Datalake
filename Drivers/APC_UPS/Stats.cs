using System;

namespace iNOPC.Drivers.APC_UPS
{
	public class Stats
	{
		public bool ACTIVE { get; set; }

		public DateTime DATE { get; set; }

		public string STATUS { get; set; }

		public string SELFTEST { get; set; }

		public string LASTXFER { get; set; }

		public string LASTSTEST { get; set; }

		public string TONBATT { get; set; }

		public string XOFFBATT { get; set; }

		public string XONBATT { get; set; }

		public double BCHARGE { get; set; }

		public double LOADPCT { get; set; }

		public double ITEMP { get; set; }

		public double LINEV { get; set; }

		public double OUTPUTV { get; set; }

		public double TIMELEFT { get; set; }
	}
}
