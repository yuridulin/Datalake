using Datalake.Domain.Entities;
using Datalake.Shared.Application.Attributes;
using Datalake.Shared.Infrastructure.Schema;
using LinqToDB;
using LinqToDB.Data;
using LinqToDB.Mapping;

namespace Datalake.Data.Infrastructure.Database;

[Scoped]
public class DataDbLinqContext(DataOptions<DataDbLinqContext> options) : DataConnection(options.Options.UseMappingSchema(_mappings))
{
	private static readonly MappingSchema _mappings;

	static DataDbLinqContext()
	{
		_mappings = new MappingSchema();
		var builder = new FluentMappingBuilder(_mappings);

		builder
			.Entity<TagValue>()
			.HasSchemaName(DataSchema.Name)
			.HasTableName(DataSchema.TagsValues.Name)
			.HasPrimaryKey(x => new { x.TagId, x.Date })
			.Property(x => x.TagId).HasColumnName(DataSchema.TagsValues.Columns.TagId)
			.Property(x => x.Date).HasColumnName(DataSchema.TagsValues.Columns.Date)
			.Property(x => x.Text).HasColumnName(DataSchema.TagsValues.Columns.Text)
			.Property(x => x.Number).HasColumnName(DataSchema.TagsValues.Columns.Number)
			.Property(x => x.Boolean).HasColumnName(DataSchema.TagsValues.Columns.Boolean)
			.Property(x => x.Quality).HasColumnName(DataSchema.TagsValues.Columns.Quality)
			.Build();
	}

	public ITable<TagValue> TagsValues
		=> this.GetTable<TagValue>();
}

