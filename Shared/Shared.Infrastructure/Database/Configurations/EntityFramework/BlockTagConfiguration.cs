using Datalake.Domain.Entities;
using Datalake.Shared.Infrastructure.Database.Schema;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Datalake.Shared.Infrastructure.Database.Configurations.EntityFramework;

public class BlockTagConfiguration(DatabaseTableAccess access) : IEntityTypeConfiguration<BlockTag>
{
	public void Configure(EntityTypeBuilder<BlockTag> builder)
	{
		if (access == DatabaseTableAccess.Read)
			builder.ToView(InventorySchema.BlockTags.Name, InventorySchema.Name);
		else
			builder.ToTable(InventorySchema.BlockTags.Name, InventorySchema.Name);

		builder.HasKey(x => x.Id);

		if (access == DatabaseTableAccess.Write)
			builder.Property(x => x.Id).ValueGeneratedOnAdd();

		var relationToTag = builder.HasOne(rel => rel.Tag)
			.WithMany(e => e.RelationsToBlocks)
			.HasForeignKey(rel => rel.TagId);

		var relationToBlock = builder.HasOne(rel => rel.Block)
			.WithMany(e => e.RelationsToTags)
			.HasForeignKey(rel => rel.BlockId);

		if (access == DatabaseTableAccess.Write)
		{
			builder.HasIndex(rel => new { rel.BlockId, rel.TagId }).IsUnique();

			relationToTag.OnDelete(DeleteBehavior.SetNull);
			relationToBlock.OnDelete(DeleteBehavior.Cascade);
		}
	}
}
