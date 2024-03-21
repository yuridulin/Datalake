using DatalakeDatabase.Enums;
using LinqToDB.Mapping;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ColumnAttribute = LinqToDB.Mapping.ColumnAttribute;
using TableAttribute = System.ComponentModel.DataAnnotations.Schema.TableAttribute;

namespace DatalakeDatabase.Models
{
	[Table(TableName), LinqToDB.Mapping.Table(TableName)]
	public class EntityField
	{
		const string TableName = "EntityFields";

		// поля в БД

		[Key, Identity, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public int Id { get; set; }

		[Column]
		public int EntityId { get; set; } = 0;

		[Column]
		public required string Name { get; set; } = string.Empty;

		[Column]
		public required string Value { get; set; } = string.Empty;

		[Column]
		public TagType Type { get; set; } = TagType.String;

		// связи

		[ForeignKey(nameof(EntityId))]
		public Entity? Entity { get; set; }
	}
}
