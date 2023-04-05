using LinqToDB;
using LinqToDB.Data;

namespace Datalake.Database
{
	public class DatabaseContext : DataConnection
	{
		public DatabaseContext() : base("Default") { }

		public ITable<Tag> Tags
			=> this.GetTable<Tag>();
	}
}
