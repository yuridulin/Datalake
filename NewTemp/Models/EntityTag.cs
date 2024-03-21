using Microsoft.EntityFrameworkCore;
using ColumnAttribute = LinqToDB.Mapping.ColumnAttribute;
using TableAttribute = System.ComponentModel.DataAnnotations.Schema.TableAttribute;

namespace DatalakeDatabase.Models
{
	[Table(TableName), LinqToDB.Mapping.Table(TableName)]
	public class EntityTag
	{
		const string TableName = "EntityTags";

		// поля в БД

		[Column]
		public int EntityId { get; set; } = 0;

		[Column]
		public int TagId { get; set; } = 0;

		[Column]
		public required string Name { get; set; } = string.Empty;

		// связи

		public Entity? Entity { get; set; }

		public Tag? Tag { get; set; }
	}
}
