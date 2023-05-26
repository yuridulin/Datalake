using iNOPC.Server.Models.Configurations;
using System;

namespace iNOPC.Server.Web
{
    public class Session
	{
		public string Login { get; set; }

		public string Token { get; set; }

		public AccessType AccessType { get; set; }

		public DateTime Expire { get; set; }
	}
}