using Datalake.Domain.Entities;
using Datalake.Shared.Infrastructure.Database.Schema;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Datalake.Shared.Infrastructure.Database.Configurations.EntityFramework;

public class BlockPropertyConfiguration(DatabaseTableAccess access) : IEntityTypeConfiguration<BlockProperty>
{
	public void Configure(EntityTypeBuilder<BlockProperty> builder)
	{
		if (access == DatabaseTableAccess.Read)
			builder.ToView(InventorySchema.BlockProperties.Name, InventorySchema.Name);
		else
			builder.ToTable(InventorySchema.BlockProperties.Name, InventorySchema.Name);

		builder.HasKey(x => x.Id);

		if (access == DatabaseTableAccess.Write)
			builder.Property(x => x.Id).ValueGeneratedOnAdd();
	}
}
