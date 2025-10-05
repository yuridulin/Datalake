using Datalake.Domain.Entities;
using Datalake.Shared.Infrastructure.Schema;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Datalake.Inventory.Infrastructure.Database.Configurations;

public class BlockConfiguration(bool isReadOnly = false) : IEntityTypeConfiguration<Block>
{
	public void Configure(EntityTypeBuilder<Block> builder)
	{
		if (isReadOnly)
			builder.ToView(InventorySchema.Blocks.Name, InventorySchema.Name);
		else
			builder.ToTable(InventorySchema.Blocks.Name, InventorySchema.Name);

		builder.HasKey(x => x.Id);

		// связь блоков по иерархии
		var hierarchyRelations = builder.HasOne(block => block.Parent)
			.WithMany(block => block.Children)
			.HasForeignKey(block => block.ParentId);

		if (!isReadOnly)
			hierarchyRelations.OnDelete(DeleteBehavior.SetNull);

		// связь блоков и тегов
		builder.HasMany(block => block.Tags)
			.WithMany()
			.UsingEntity<BlockTag>();
	}
}
