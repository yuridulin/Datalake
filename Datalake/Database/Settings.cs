using LinqToDB.Mapping;
using System;

namespace Datalake.Database
{
	[Table(Name = "Settings")]
	public class Settings
	{
		[Column]
		public DateTime LastUpdate { get; set; }
	}
}
