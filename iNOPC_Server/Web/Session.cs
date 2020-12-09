using iNOPC.Server.Models;
using System;

namespace iNOPC.Server.Web
{
	public class Session
	{
		public string Token { get; set; }

		public AccessTypes AccessType { get; set; }

		public DateTime Expire { get; set; }
	}
}