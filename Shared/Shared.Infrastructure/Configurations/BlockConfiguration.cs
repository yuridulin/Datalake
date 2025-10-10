using Datalake.Domain.Entities;
using Datalake.Shared.Infrastructure.Schema;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using static Datalake.Shared.Infrastructure.ConfigurationsApplyHelper;

namespace Datalake.Shared.Infrastructure.Configurations;

public class BlockConfiguration(TableAccess access) : IEntityTypeConfiguration<Block>
{
	public void Configure(EntityTypeBuilder<Block> builder)
	{
		if (access == TableAccess.Read)
			builder.ToView(InventorySchema.Blocks.Name, InventorySchema.Name);
		else
			builder.ToTable(InventorySchema.Blocks.Name, InventorySchema.Name);

		builder.HasKey(x => x.Id);

		// связь блоков по иерархии
		var hierarchyRelations = builder.HasOne(block => block.Parent)
			.WithMany(block => block.Children)
			.HasForeignKey(block => block.ParentId);

		if (access == TableAccess.Write)
			hierarchyRelations.OnDelete(DeleteBehavior.SetNull);

		// связь блоков и тегов
		var relationToTag = builder.HasMany(block => block.RelationsToTags)
			.WithOne(r => r.Block)
			.HasForeignKey(r => r.BlockId);

		if (access == TableAccess.Write)
			relationToTag.OnDelete(DeleteBehavior.Cascade);
	}
}
