using Datalake.Enums;
using LinqToDB;
using LinqToDB.Mapping;
using System;

namespace Datalake.Database
{
	[Table(Name = "Logs")]
	public class Log
	{
		[Column, NotNull, DataType(DataType.DateTime2)]
		public DateTime Date { get; set; } = DateTime.Now;

		[Column, NotNull]
		public LogCategory Category { get; set; } = LogCategory.Api;

		[Column]
		public int? Ref { get; set; }

		[Column, NotNull]
		public LogType Type { get; set; } = LogType.Information;

		[Column, NotNull]
		public string Text { get; set; } = string.Empty;

		[Column]
		public string Details { get; set; }

		public Exception Exception
		{
			set
			{
				Details = value.Message + "\n" + value.StackTrace;
			}
		}
	}
}
