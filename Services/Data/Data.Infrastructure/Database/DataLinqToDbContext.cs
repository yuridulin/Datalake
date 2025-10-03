using Datalake.Data.Infrastructure.Database.Constants;
using Datalake.Domain.ValueObjects;
using LinqToDB;
using LinqToDB.Data;
using LinqToDB.Mapping;

namespace Datalake.Data.Infrastructure.Database;

public class DataLinqToDbContext(DataOptions<DataLinqToDbContext> options) : DataConnection(options.Options.UseMappingSchema(_mappings))
{
	private static readonly MappingSchema _mappings;

	static DataLinqToDbContext()
	{
		_mappings = new MappingSchema();
		var builder = new FluentMappingBuilder(_mappings);

		builder
			.Entity<TagHistoryValue>()
			.HasSchemaName(DataDefinition.Schema)
			.HasTableName(DataDefinition.TagHistory.Table)
			.HasPrimaryKey(x => new { x.TagId, x.Date })
			.Property(x => x.TagId).HasColumnName(DataDefinition.TagHistory.TagId)
			.Property(x => x.Date).HasColumnName(DataDefinition.TagHistory.Date)
			.Property(x => x.Text).HasColumnName(DataDefinition.TagHistory.Text)
			.Property(x => x.Number).HasColumnName(DataDefinition.TagHistory.Number)
			.Property(x => x.Quality).HasColumnName(DataDefinition.TagHistory.Quality)
			.Build();
	}

	public ITable<TagHistoryValue> TagsHistory
		=> this.GetTable<TagHistoryValue>();
}
