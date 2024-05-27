using DatalakeApiClasses.Enums;
using LinqToDB.Mapping;
using Microsoft.EntityFrameworkCore;
using ColumnAttribute = LinqToDB.Mapping.ColumnAttribute;
using TableAttribute = System.ComponentModel.DataAnnotations.Schema.TableAttribute;

namespace DatalakeDatabase.Models;

[Keyless, Table(TableName), LinqToDB.Mapping.Table(TableName)]
public class TagHistory
{
	const string TableName = "TagsLive";

	[Column, NotNull]
	public int TagId { get; set; }

	[Column, NotNull]
	public DateTime Date { get; set; } = DateTime.Now;

	[Column, Nullable]
	public string? Text { get; set; } = null;

	[Column, Nullable]
	public float? Number { get; set; } = null;

	[Column, NotNull]
	public TagQuality Quality { get; set; } = TagQuality.Good;

	[Column, NotNull]
	public TagUsing Using { get; set; } = TagUsing.Basic;
}
