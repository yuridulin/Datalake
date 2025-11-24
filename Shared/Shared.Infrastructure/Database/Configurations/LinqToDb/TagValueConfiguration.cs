using Datalake.Domain.Entities;
using Datalake.Shared.Infrastructure.Database.Schema;
using LinqToDB.Mapping;

namespace Datalake.Shared.Infrastructure.Database.Configurations.LinqToDb;

public class TagValueConfiguration(DatabaseTableAccess access) : ILinqToDbEntityConfiguration<TagValue>
{
	public void Configure(EntityMappingBuilder<TagValue> builder)
	{
		_ = access;

		builder
			.HasSchemaName(DataSchema.Name)
			.HasTableName(DataSchema.TagsValues.Name)
			.HasPrimaryKey(x => new { x.TagId, x.Date })
			.Property(x => x.TagId).HasColumnName(DataSchema.TagsValues.Columns.TagId)
			.Property(x => x.Date).HasColumnName(DataSchema.TagsValues.Columns.Date)
			.Property(x => x.Text).HasColumnName(DataSchema.TagsValues.Columns.Text)
			.Property(x => x.Number).HasColumnName(DataSchema.TagsValues.Columns.Number)
			.Property(x => x.Boolean).HasColumnName(DataSchema.TagsValues.Columns.Boolean)
			.Property(x => x.Quality).HasColumnName(DataSchema.TagsValues.Columns.Quality);
	}
}
