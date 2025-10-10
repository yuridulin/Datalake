using Datalake.Domain.Entities;
using Datalake.Shared.Infrastructure.Schema;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using static Datalake.Shared.Infrastructure.ConfigurationsApplyHelper;

namespace Datalake.Shared.Infrastructure.Configurations;

public class BlockPropertyConfiguration(TableAccess access) : IEntityTypeConfiguration<BlockProperty>
{
	public void Configure(EntityTypeBuilder<BlockProperty> builder)
	{
		if (access == TableAccess.Read)
			builder.ToView(InventorySchema.BlockProperties.Name, InventorySchema.Name);
		else
			builder.ToTable(InventorySchema.BlockProperties.Name, InventorySchema.Name);

		builder.HasKey(x => x.Id);

		if (access == TableAccess.Write)
			builder.Property(x => x.Id).ValueGeneratedOnAdd();
	}
}
