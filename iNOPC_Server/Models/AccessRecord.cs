namespace iNOPC.Server.Models
{
	public class AccessRecord
	{
		public string Login { get; set; }

		public string Hash { get; set; }

		public AccessTypes AccessType { get; set; }
	}
}