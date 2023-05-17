using LinqToDB;
using LinqToDB.Data;
using System.Linq;

namespace Logger.Database
{
	public class DatabaseContext : DataConnection
	{
		public DatabaseContext() : base("Default") { }

		public ITable<LogEntry> Logs
			=> this.GetTable<LogEntry>();

		public ITable<Agent> Agents
			=> this.GetTable<Agent>();

		public ITable<Preset> Presets
			=> this.GetTable<Preset>();

		public ITable<Filter> Filters
			=> this.GetTable<Filter>();

		public void Setup()
		{
			var provider = DataProvider.GetSchemaProvider();
			var schema = provider.GetSchema(this);

			if (!schema.Tables.Any(x => x.TableName == Logs.TableName))
			{
				this.CreateTable<LogEntry>();
			}

			if (!schema.Tables.Any(x => x.TableName == Agents.TableName))
			{
				this.CreateTable<Agent>();
			}

			if (!schema.Tables.Any(x => x.TableName == Presets.TableName))
			{
				this.CreateTable<Preset>();
			}

			if (!schema.Tables.Any(x => x.TableName == Filters.TableName))
			{
				this.CreateTable<Filter>();
			}
		}
	}
}
