using LinqToDB.Mapping;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ColumnAttribute = LinqToDB.Mapping.ColumnAttribute;
using TableAttribute = System.ComponentModel.DataAnnotations.Schema.TableAttribute;

namespace Datalake.Database.Models;

[Table(TableName), LinqToDB.Mapping.Table(TableName)]
public class Block
{
	const string TableName = "Blocks";

	// поля в БД

	[Key, Identity, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
	public int Id { get; set; }

	[Column]
	public Guid GlobalId { get; set; }

	[Column]
	public int? ParentId { get; set; }

	[Column]
	public string Name { get; set; } = string.Empty;

	[Column]
	public string? Description { get; set; }

	// связанные данные

	[ForeignKey(nameof(ParentId))]
	public Block? Parent { get; set; }

	[InverseProperty(nameof(Parent))]
	public ICollection<Block> Children { get; set; } = [];

	[InverseProperty(nameof(BlockProperty.Block))]
	public ICollection<BlockProperty> Properties { get; set; } = [];

	public ICollection<BlockTag> RelationsToTags { get; set; } = [];

	public ICollection<Tag> Tags { get; set; } = [];

	[InverseProperty(nameof(AccessRights.Block))]
	public ICollection<AccessRights> AccessRightsList { get; set; } = [];
}
