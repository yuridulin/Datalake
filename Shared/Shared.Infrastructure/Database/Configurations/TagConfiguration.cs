using Datalake.Domain.Entities;
using Datalake.Shared.Infrastructure.Database.Schema;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Datalake.Shared.Infrastructure.Database.Configurations;

public class TagConfiguration(DatabaseTableAccess access) : IEntityTypeConfiguration<Tag>
{
	public void Configure(EntityTypeBuilder<Tag> builder)
	{
		if (access == DatabaseTableAccess.Read)
			builder.ToView(InventorySchema.Tags.Name, InventorySchema.Name);
		else
			builder.ToTable(InventorySchema.Tags.Name, InventorySchema.Name);

		builder.HasKey(x => x.Id);

		if (access == DatabaseTableAccess.Write)
			builder.Property(x => x.Id).ValueGeneratedOnAdd();

		// связь тегов с входными тегами (переменными)
		var relationToThresholds = builder.HasMany(tag => tag.Thresholds)
			.WithOne(threshold => threshold.Tag)
			.HasForeignKey(threshold => threshold.TagId);

		// связь источников и тегов
		var relationToSource = builder.HasOne(tag => tag.Source)
			.WithMany(source => source.Tags)
			.HasForeignKey(tag => tag.SourceId);

		// связь тегов с входными тегами (переменными)
		var relationToInputs = builder.HasMany(tag => tag.Inputs)
			.WithOne(input => input.Tag)
			.HasForeignKey(input => input.TagId);

		// связь тегов с входными тегами для агрегирования
		var relationToAggregateSourceTag = builder.HasOne(tag => tag.AggregationSourceTag)
			.WithMany(tag => tag.AggregateTagsUsingThisTag)
			.HasForeignKey(tag => tag.SourceTagId);

		// связь тегов с входными тегами для расчета
		var relationToThresholdSourceTag = builder.HasOne(tag => tag.ThresholdsSourceTag)
			.WithMany(tag => tag.ThresholdsTagsUsingThisTag)
			.HasForeignKey(tag => tag.ThresholdSourceTagId);

		// связь блоков и тегов
		var relationToBlock = builder.HasMany(tag => tag.RelationsToBlocks)
			.WithOne(r => r.Tag)
			.HasForeignKey(r => r.TagId);

		if (access == DatabaseTableAccess.Write)
			relationToBlock.OnDelete(DeleteBehavior.SetNull);

		if (access == DatabaseTableAccess.Write)
		{
			relationToThresholds.OnDelete(DeleteBehavior.Cascade);
			relationToSource.OnDelete(DeleteBehavior.SetNull);
			relationToInputs.OnDelete(DeleteBehavior.Cascade);
			relationToAggregateSourceTag.OnDelete(DeleteBehavior.SetNull);
			relationToThresholdSourceTag.OnDelete(DeleteBehavior.SetNull);
		}
	}
}
