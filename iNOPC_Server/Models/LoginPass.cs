using iNOPC.Server.Models;
using System;
using System.Security.Cryptography;
using System.Text;

namespace iNOPC.Server.Web
{
	public class LoginPass
	{
		public string Login { get; set; }

		public string Password { get; set; }

		public AccessType AccessType { get; set; } = AccessType.GUEST;

		public string Hash
		{
			get
			{
				var sha1 = new SHA1CryptoServiceProvider();
				var hash = sha1.ComputeHash(Encoding.UTF8.GetBytes(Password));
				return Convert.ToBase64String(hash);
			}
		}
	}
}