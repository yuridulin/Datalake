using Datalake.Domain.ValueObjects;
using Datalake.Shared.Infrastructure.Schema;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using static Datalake.Shared.Infrastructure.ConfigurationsApplyHelper;

namespace Datalake.Shared.Infrastructure.Configurations;

public class TagHistoryConfiguration(TableAccess access) : IEntityTypeConfiguration<TagHistoryValue>
{
	public void Configure(EntityTypeBuilder<TagHistoryValue> builder)
	{
		if (access == TableAccess.Read)
			builder.ToView(DataSchema.TagsHistory.Name, DataSchema.Name);
		else
			builder.ToTable(DataSchema.TagsHistory.Name, DataSchema.Name);

		builder.HasNoKey();

		builder.Property(x => x.TagId).HasColumnName(DataSchema.TagsHistory.Columns.TagId);
		builder.Property(x => x.Date).HasColumnName(DataSchema.TagsHistory.Columns.Date);
		builder.Property(x => x.Text).HasColumnName(DataSchema.TagsHistory.Columns.Text);
		builder.Property(x => x.Number).HasColumnName(DataSchema.TagsHistory.Columns.Number);
		builder.Property(x => x.Boolean).HasColumnName(DataSchema.TagsHistory.Columns.Boolean);
		builder.Property(x => x.Quality).HasColumnName(DataSchema.TagsHistory.Columns.Quality);

		// уникальность значений тегов
		builder.HasIndex(record => new { record.TagId, record.Date })
			.HasDatabaseName(DataSchema.TagsHistory.Indexes.UniqueTagIdDateDesc)
			.IsDescending([false, true])
			.IsUnique();
	}
}