using Datalake.Database.Enums;
using LinqToDB.Mapping;
using System;

namespace Datalake.Database
{
	[Table(Name = "ProgramLogs")]
	public class ProgramLog
	{
		[Column, Identity, PrimaryKey]
		public long Id { get; set; }

		[Column, NotNull]
		public string Module { get; set; }

		[Column, NotNull]
		public DateTime Timestamp { get; set; } = DateTime.Now;

		[Column]
		public string Message { get; set; }

		[Column, NotNull]
		public ProgramLogType Type { get; set; } = ProgramLogType.Trace;
	}
}
