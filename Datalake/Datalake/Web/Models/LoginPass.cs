using Datalake.Enums;
using System;
using System.Security.Cryptography;
using System.Security.Policy;
using System.Text;

namespace Datalake.Web.Models
{
	public class LoginPass
	{
		public string Name { get; set; } = string.Empty;

		public string Password { get; set; } = string.Empty;

		public AccessType AccessType { get; set; } = AccessType.NOT;

		public string Hash
		{
			get
			{
				var sha1 = new SHA1CryptoServiceProvider();
				var hash = sha1.ComputeHash(Encoding.UTF8.GetBytes(Password));
				return Convert.ToBase64String(hash);
			}
		}

		public static string RandomHash()
		{
			var bytes = new byte[48];
			using (var rng = new RNGCryptoServiceProvider())
			{
				rng.GetBytes(bytes);
			}
			return BitConverter.ToString(bytes).Replace("-", "").ToLower();
		}
	}
}