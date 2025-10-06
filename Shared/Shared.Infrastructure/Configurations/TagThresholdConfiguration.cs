using Datalake.Domain.Entities;
using Datalake.Shared.Infrastructure.Schema;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Datalake.Shared.Infrastructure.Configurations;

public class TagThresholdConfiguration(bool isReadOnly = false) : IEntityTypeConfiguration<TagThreshold>
{
	public void Configure(EntityTypeBuilder<TagThreshold> builder)
	{
		if (isReadOnly)
			builder.ToView(InventorySchema.TagThresholds.Name, InventorySchema.Name);
		else
			builder.ToTable(InventorySchema.TagThresholds.Name, InventorySchema.Name);

		builder.HasKey(x => x.Id);

		if (!isReadOnly)
			builder.Property(x => x.Id).ValueGeneratedOnAdd();
	}
}
