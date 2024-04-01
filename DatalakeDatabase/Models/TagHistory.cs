using DatalakeDatabase.Enums;
using LinqToDB.Mapping;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;
using ColumnAttribute = LinqToDB.Mapping.ColumnAttribute;

namespace DatalakeDatabase.Models;

[Keyless, NotMapped]
public class TagHistory
{
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
