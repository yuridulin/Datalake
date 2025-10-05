using Datalake.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Datalake.Inventory.Infrastructure.Database.Configurations;

public class BlockEntityConfiguration : IEntityTypeConfiguration<Block>
{
	public void Configure(EntityTypeBuilder<Block> builder)
	{
		builder.HasKey(x => x.Id);

		// связь блоков по иерархии
		builder.HasOne(block => block.Parent)
			.WithMany(block => block.Children)
			.HasForeignKey(block => block.ParentId)
			.OnDelete(DeleteBehavior.SetNull);

		// связь блоков и тегов
		builder.HasMany(block => block.Tags)
			.WithMany(tag => tag.Blocks)
			.UsingEntity<BlockTag>(
				relation => relation
					.HasOne(rel => rel.Tag)
					.WithMany(tag => tag.RelationsToBlocks)
					.HasForeignKey(rel => rel.TagId)
					.OnDelete(DeleteBehavior.SetNull),
				relation => relation
					.HasOne(rel => rel.Block)
					.WithMany(e => e.RelationsToTags)
					.HasForeignKey(rel => rel.BlockId)
					.OnDelete(DeleteBehavior.Cascade),
				relation => relation
					.HasKey(rel => new { rel.BlockId, rel.TagId })
			);
	}
}
