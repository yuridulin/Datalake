using Datalake.Domain.Entities;
using Datalake.Shared.Infrastructure.Schema;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using static Datalake.Shared.Infrastructure.ConfigurationsApplyHelper;

namespace Datalake.Shared.Infrastructure.Configurations;

public class BlockTagConfiguration(TableAccess access) : IEntityTypeConfiguration<BlockTag>
{
	public void Configure(EntityTypeBuilder<BlockTag> builder)
	{
		if (access == TableAccess.Read)
			builder.ToView(InventorySchema.BlockTags.Name, InventorySchema.Name);
		else
			builder.ToTable(InventorySchema.BlockTags.Name, InventorySchema.Name);

		builder.HasKey(x => x.Id);

		if (access == TableAccess.Write)
			builder.Property(x => x.Id).ValueGeneratedOnAdd();

		var relationToTag = builder.HasOne(rel => rel.Tag)
			.WithMany(e => e.RelationsToBlocks)
			.HasForeignKey(rel => rel.TagId);

		var relationToBlock = builder.HasOne(rel => rel.Block)
			.WithMany(e => e.RelationsToTags)
			.HasForeignKey(rel => rel.BlockId);

		builder.HasIndex(rel => new { rel.BlockId, rel.TagId }).IsUnique();

		if (access == TableAccess.Write)
		{
			relationToTag.OnDelete(DeleteBehavior.SetNull);
			relationToBlock.OnDelete(DeleteBehavior.Cascade);
		}
	}
}
