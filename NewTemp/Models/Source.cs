using DatalakeDatabase.Enums;
using LinqToDB.Mapping;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ColumnAttribute = LinqToDB.Mapping.ColumnAttribute;
using TableAttribute = System.ComponentModel.DataAnnotations.Schema.TableAttribute;

namespace DatalakeDatabase.Models
{
	[Table(TableName), LinqToDB.Mapping.Table(TableName)]
	public class Source
	{
		const string TableName = "Sources";

		// поля в БД

		[Key, Identity, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public int Id { get; set; }

		[Column]
		public required string Name { get; set; } = string.Empty;

		[Column]
		public SourceType Type { get; set; } = SourceType.Inopc;

		[Column, Nullable]
		public string? Address { get; set; }

		// связи

		[InverseProperty(nameof(Tag.Source))]
		public ICollection<Tag> Tags { get; set; } = [];
	}
}
