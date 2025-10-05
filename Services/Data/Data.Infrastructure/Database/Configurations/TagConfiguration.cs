using Datalake.Domain.Entities;
using Datalake.Shared.Infrastructure.Schema;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Datalake.Data.Infrastructure.Database.Configurations;

public class TagConfiguration : IEntityTypeConfiguration<Tag>
{
	public void Configure(EntityTypeBuilder<Tag> builder)
	{
		builder.ToView(InventorySchema.Tags.Name, InventorySchema.Name);

		builder.HasKey(x => x.Id);

		// связь тегов с входными тегами (переменными)
		builder.HasMany(tag => tag.Thresholds)
			.WithOne(threshold => threshold.Tag)
			.HasForeignKey(threshold => threshold.TagId);

		// связь тегов с входными тегами (переменными)
		builder.HasMany(tag => tag.Inputs)
			.WithOne(input => input.Tag)
			.HasForeignKey(input => input.TagId);

		// связь тегов с входными тегами для агрегирования
		builder.HasOne(tag => tag.SourceTag)
			.WithMany()
			.HasForeignKey(tag => tag.SourceTagId);

		// связь тегов с входными тегами для расчета
		builder.HasOne(tag => tag.ThresholdSourceTag)
			.WithMany()
			.HasForeignKey(tag => tag.ThresholdSourceTagId);
	}
}
