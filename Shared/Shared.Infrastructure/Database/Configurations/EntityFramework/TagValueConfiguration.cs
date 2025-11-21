using Datalake.Domain.Entities;
using Datalake.Shared.Infrastructure.Database.Schema;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Datalake.Shared.Infrastructure.Database.Configurations.EntityFramework;

public class TagValueConfiguration(DatabaseTableAccess access) : IEntityTypeConfiguration<TagValue>
{
	public void Configure(EntityTypeBuilder<TagValue> builder)
	{
		if (access == DatabaseTableAccess.Read)
			builder.ToView(DataSchema.TagsValues.Name, DataSchema.Name);
		else
			builder.ToTable(DataSchema.TagsValues.Name, DataSchema.Name);

		builder.HasKey(record => new { record.TagId, record.Date });

		builder.Property(x => x.TagId).HasColumnName(DataSchema.TagsValues.Columns.TagId);
		builder.Property(x => x.Date).HasColumnName(DataSchema.TagsValues.Columns.Date);
		builder.Property(x => x.Text).HasColumnName(DataSchema.TagsValues.Columns.Text);
		builder.Property(x => x.Number).HasColumnName(DataSchema.TagsValues.Columns.Number);
		builder.Property(x => x.Boolean).HasColumnName(DataSchema.TagsValues.Columns.Boolean);
		builder.Property(x => x.Quality).HasColumnName(DataSchema.TagsValues.Columns.Quality);
	}
}