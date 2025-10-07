using Datalake.Domain.ValueObjects;
using Datalake.Shared.Application.Attributes;
using Datalake.Shared.Infrastructure.Schema;
using LinqToDB;
using LinqToDB.Data;
using LinqToDB.Mapping;

namespace Datalake.Data.Infrastructure.Database;

[Scoped]
public class DataLinqToDbContext(DataOptions<DataLinqToDbContext> options) : DataConnection(options.Options.UseMappingSchema(_mappings))
{
	private static readonly MappingSchema _mappings;

	static DataLinqToDbContext()
	{
		_mappings = new MappingSchema();
		var builder = new FluentMappingBuilder(_mappings);

		builder
			.Entity<TagHistory>()
			.HasSchemaName(DataSchema.Name)
			.HasTableName(DataSchema.TagsHistory.Name)
			.HasPrimaryKey(x => new { x.TagId, x.Date })
			.Property(x => x.TagId).HasColumnName(DataSchema.TagsHistory.Columns.TagId)
			.Property(x => x.Date).HasColumnName(DataSchema.TagsHistory.Columns.Date)
			.Property(x => x.Text).HasColumnName(DataSchema.TagsHistory.Columns.Text)
			.Property(x => x.Number).HasColumnName(DataSchema.TagsHistory.Columns.Number)
			.Property(x => x.Boolean).HasColumnName(DataSchema.TagsHistory.Columns.Boolean)
			.Property(x => x.Quality).HasColumnName(DataSchema.TagsHistory.Columns.Quality)
			.Build();
	}

	public ITable<TagHistory> TagsHistory
		=> this.GetTable<TagHistory>();
}

