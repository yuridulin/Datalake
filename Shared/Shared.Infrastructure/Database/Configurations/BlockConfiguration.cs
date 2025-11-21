using Datalake.Domain.Entities;
using Datalake.Shared.Infrastructure.Database.Schema;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Datalake.Shared.Infrastructure.Database.Configurations;

public class BlockConfiguration(DatabaseTableAccess access) : IEntityTypeConfiguration<Block>
{
	public void Configure(EntityTypeBuilder<Block> builder)
	{
		if (access == DatabaseTableAccess.Read)
			builder.ToView(InventorySchema.Blocks.Name, InventorySchema.Name);
		else
			builder.ToTable(InventorySchema.Blocks.Name, InventorySchema.Name);

		builder.HasKey(x => x.Id);

		// связь блоков по иерархии
		var hierarchyRelations = builder.HasOne(block => block.Parent)
			.WithMany(block => block.Children)
			.HasForeignKey(block => block.ParentId);

		if (access == DatabaseTableAccess.Write)
			hierarchyRelations.OnDelete(DeleteBehavior.SetNull);

		// связь блоков и тегов
		var relationToTag = builder.HasMany(block => block.RelationsToTags)
			.WithOne(r => r.Block)
			.HasForeignKey(r => r.BlockId);

		if (access == DatabaseTableAccess.Write)
			relationToTag.OnDelete(DeleteBehavior.Cascade);
	}
}
