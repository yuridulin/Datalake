using Datalake.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Datalake.Inventory.Infrastructure.Database.Configurations;

public class TagEntityConfiguration : IEntityTypeConfiguration<TagEntity>
{
	public void Configure(EntityTypeBuilder<TagEntity> builder)
	{
		builder.HasKey(x => x.Id);

		// связь тегов с входными тегами (переменными)
		/*builder.HasMany(tag => tag.Thresholds)
			.WithOne(threshold => threshold.Tag)
			.HasForeignKey(threshold => threshold.TagId)
			.OnDelete(DeleteBehavior.Cascade);*/

		// связь источников и тегов
		builder.HasOne(tag => tag.Source)
			.WithMany(source => source.Tags)
			.HasForeignKey(tag => tag.SourceId)
			.OnDelete(DeleteBehavior.SetNull);

		// связь тегов с входными тегами (переменными)
		builder.HasMany(tag => tag.Inputs)
			.WithOne(input => input.Tag)
			.HasForeignKey(input => input.TagId)
			.OnDelete(DeleteBehavior.Cascade);

		// связь тегов с входными тегами для агрегирования
		builder.HasOne(tag => tag.SourceTag)
			.WithMany()
			.HasForeignKey(tag => tag.SourceTagId)
			.OnDelete(DeleteBehavior.SetNull);

		// связь тегов с входными тегами для расчета
		builder.HasOne(tag => tag.ThresholdSourceTag)
			.WithMany()
			.HasForeignKey(tag => tag.ThresholdSourceTagId)
			.OnDelete(DeleteBehavior.SetNull);
	}
}
