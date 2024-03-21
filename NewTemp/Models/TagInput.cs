using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;
using ColumnAttribute = LinqToDB.Mapping.ColumnAttribute;
using TableAttribute = System.ComponentModel.DataAnnotations.Schema.TableAttribute;

namespace DatalakeDatabase.Models
{
	[Keyless, Table(TableName), LinqToDB.Mapping.Table(TableName)]
	public class TagInput
	{
		const string TableName = "TagInputs";

		// поля в БД

		[Column]
		public int ResultTagId { get; set; } = 0;

		[Column]
		public int InputTagId { get; set; } = 0;

		[Column]
		public required string VariableName { get; set; } = string.Empty;

		// связи

		[NotMapped]
		public Tag? ResultTag { get; set; }

		[NotMapped]
		public Tag? InputTag { get; set; }
	}
}
