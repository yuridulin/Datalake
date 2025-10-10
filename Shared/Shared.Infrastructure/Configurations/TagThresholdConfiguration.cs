using Datalake.Domain.Entities;
using Datalake.Shared.Infrastructure.Schema;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using static Datalake.Shared.Infrastructure.ConfigurationsApplyHelper;

namespace Datalake.Shared.Infrastructure.Configurations;

public class TagThresholdConfiguration(TableAccess access) : IEntityTypeConfiguration<TagThreshold>
{
	public void Configure(EntityTypeBuilder<TagThreshold> builder)
	{
		if (access == TableAccess.Read)
			builder.ToView(InventorySchema.TagThresholds.Name, InventorySchema.Name);
		else
			builder.ToTable(InventorySchema.TagThresholds.Name, InventorySchema.Name);

		builder.HasKey(x => x.Id);

		if (access == TableAccess.Write)
			builder.Property(x => x.Id).ValueGeneratedOnAdd();
	}
}
