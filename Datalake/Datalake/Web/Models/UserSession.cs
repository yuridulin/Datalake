using Datalake.Enums;
using System;

namespace Datalake.Web.Models
{
	public class UserSession
	{
		public string Name { get; set; }

		public string Token { get; set; }

		public AccessType AccessType { get; set; }

		public DateTime Expire { get; set; }
	}
}
