namespace iNOPC.Server.Web
{
	public class User
	{
		public string Login { get; set; }

		public string Password { get; set; }

		public AccessTypes AccessType { get; set; }
	}
}