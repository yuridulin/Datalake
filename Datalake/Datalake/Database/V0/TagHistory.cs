using Datalake.Enums;
using LinqToDB;
using LinqToDB.Mapping;
using System;

namespace Datalake.Database.V0
{
	[Table]
	public class TagHistory
	{
		[Column, DataType(DataType.Int32)]
		public int TagId { get; set; }

		[Column, DataType(DataType.DateTime2)]
		public DateTime Date { get; set; }

		[Column, DataType(DataType.VarChar)]
		public string Text { get; set; }

		[Column]
		public float? Number { get; set; }

		[Column, DataType(DataType.Byte)]
		public TagQuality Quality { get; set; }

		[Column, DataType(DataType.Byte)]
		public TagType Type { get; set; }

		[Column, DataType(DataType.Byte)]
		public TagHistoryUse Using { get; set; }
	}
}
