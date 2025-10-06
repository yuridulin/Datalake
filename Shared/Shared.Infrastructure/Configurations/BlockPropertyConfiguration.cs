using Datalake.Domain.Entities;
using Datalake.Shared.Infrastructure.Schema;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Datalake.Shared.Infrastructure.Configurations;

public class BlockPropertyConfiguration(bool isReadOnly = false) : IEntityTypeConfiguration<BlockProperty>
{
	public void Configure(EntityTypeBuilder<BlockProperty> builder)
	{
		if (isReadOnly)
			builder.ToView(InventorySchema.BlockProperties.Name, InventorySchema.Name);
		else
			builder.ToTable(InventorySchema.BlockProperties.Name, InventorySchema.Name);

		builder.HasKey(x => x.Id);

		if (!isReadOnly)
			builder.Property(x => x.Id).ValueGeneratedOnAdd();
	}
}
