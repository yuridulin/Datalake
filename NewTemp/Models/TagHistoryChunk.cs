using LinqToDB.Mapping;
using Microsoft.EntityFrameworkCore;
using TableAttribute = System.ComponentModel.DataAnnotations.Schema.TableAttribute;

namespace DatalakeDatabase.Models
{
	[Keyless, Table(TableName), LinqToDB.Mapping.Table(TableName)]
	public class TagHistoryChunk
	{
		const string TableName = "TagHistoryChunks";

		// поля в БД

		[Column]
		public required string Table { get; set; } = string.Empty;

		[Column]
		public DateOnly Date { get; set; } = DateOnly.MinValue;
	}
}
