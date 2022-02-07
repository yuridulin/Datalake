namespace iNOPC.Server.Models.Configurations
{
	public class AccessRecord
	{
		public string Login { get; set; }

		public string Hash { get; set; }

		public AccessType AccessType { get; set; }
	}
}