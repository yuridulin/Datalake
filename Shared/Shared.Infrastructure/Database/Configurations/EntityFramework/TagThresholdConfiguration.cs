using Datalake.Domain.Entities;
using Datalake.Shared.Infrastructure.Database.Schema;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Datalake.Shared.Infrastructure.Database.Configurations.EntityFramework;

public class TagThresholdConfiguration(DatabaseTableAccess access) : IEntityTypeConfiguration<TagThreshold>
{
	public void Configure(EntityTypeBuilder<TagThreshold> builder)
	{
		if (access == DatabaseTableAccess.Read)
			builder.ToView(InventorySchema.TagThresholds.Name, InventorySchema.Name);
		else
			builder.ToTable(InventorySchema.TagThresholds.Name, InventorySchema.Name);

		builder.HasKey(x => x.Id);

		if (access == DatabaseTableAccess.Write)
			builder.Property(x => x.Id).ValueGeneratedOnAdd();
	}
}
