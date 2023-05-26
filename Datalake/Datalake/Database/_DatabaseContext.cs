using LinqToDB;
using LinqToDB.Data;
using System;
using System.Linq;

namespace Datalake.Database
{
	public class DatabaseContext : DataConnection
	{
		public DatabaseContext() : base("Default") { }

		public ITable<Tag> Tags
			=> this.GetTable<Tag>();

		public ITable<TagHistory> TagsHistory
			=> this.GetTable<TagHistory>();

		public ITable<TagLive> TagsLive
			=> this.GetTable<TagLive>();

		public ITable<Source> Sources
			=> this.GetTable<Source>();

		public ITable<Settings> Settings
			=> this.GetTable<Settings>();


		public void SetUpdateDate()
		{
			if (Settings.Count() == 0)
			{
				this.Insert(new Settings
				{
					LastUpdate = DateTime.Now,
				});
			}
			else
			{
				Settings
					.Set(x => x.LastUpdate, DateTime.Now)
					.Update();
			}
		}

		public DateTime GetUpdateDate()
		{
			return Settings.FirstOrDefault()?.LastUpdate ?? DateTime.MinValue;
		}
	}
}
