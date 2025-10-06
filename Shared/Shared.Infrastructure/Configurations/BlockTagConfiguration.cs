using Datalake.Domain.Entities;
using Datalake.Shared.Infrastructure.Schema;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Datalake.Shared.Infrastructure.Configurations;

public class BlockTagConfiguration(bool isReadOnly = false) : IEntityTypeConfiguration<BlockTag>
{
	public void Configure(EntityTypeBuilder<BlockTag> builder)
	{
		if (isReadOnly)
			builder.ToView(InventorySchema.BlockTags.Name, InventorySchema.Name);
		else
			builder.ToTable(InventorySchema.BlockTags.Name, InventorySchema.Name);

		builder.HasKey(x => x.Id);

		if (!isReadOnly)
			builder.Property(x => x.Id).ValueGeneratedOnAdd();

		var relationToTag = builder.HasOne(rel => rel.Tag)
			.WithMany(e => e.RelationsToBlocks)
			.HasForeignKey(rel => rel.TagId);

		var relationToBlock = builder.HasOne(rel => rel.Block)
			.WithMany(e => e.RelationsToTags)
			.HasForeignKey(rel => rel.BlockId);

		builder.HasIndex(rel => new { rel.BlockId, rel.TagId }).IsUnique();

		if (!isReadOnly)
		{
			relationToTag.OnDelete(DeleteBehavior.SetNull);
			relationToBlock.OnDelete(DeleteBehavior.Cascade);
		}
	}
}
