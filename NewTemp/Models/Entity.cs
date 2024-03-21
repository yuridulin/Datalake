using LinqToDB.Mapping;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ColumnAttribute = LinqToDB.Mapping.ColumnAttribute;
using TableAttribute = System.ComponentModel.DataAnnotations.Schema.TableAttribute;

namespace DatalakeDatabase.Models
{
	[Table(TableName), LinqToDB.Mapping.Table(TableName)]
	public class Entity
	{
		const string TableName = "Entities";

		// поля в БД

		[Key, Identity, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public int Id { get; set; }

		[Column]
		public int ParentId { get; set; } = 0;

		[Column]
		public Guid GlobalId { get; set; }

		[Column]
		public required string Name { get; set; } = string.Empty;

		[Column]
		public required string Description { get; set; } = string.Empty;

		// связанные данные

		[NotMapped]
		public ICollection<Entity> Children { get; set; } = [];

		[InverseProperty(nameof(EntityField.Entity))]
		public ICollection<EntityField> Fields { get; set; } = [];

		public ICollection<EntityTag> RelatedTags { get; set; } = [];

		public ICollection<Tag> Tags { get; set; } = [];
	}
}
