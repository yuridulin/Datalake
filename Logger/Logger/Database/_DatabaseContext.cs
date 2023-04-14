using LinqToDB;
using LinqToDB.Data;
using System;
using System.Linq;

namespace Logger.Database
{
	public class DatabaseContext : DataConnection
	{
		public DatabaseContext() : base("Default") { }

		public ITable<LogEntry> Logs
			=> this.GetTable<LogEntry>();

		public void Setup()
		{
			var provider = DataProvider.GetSchemaProvider();
			var schema = provider.GetSchema(this);

			if (!schema.Tables.Any(x => x.TableName == Logs.TableName))
			{
				this.CreateTable<LogEntry>();
			}
		}
	}
}
