using Datalake.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Datalake.Inventory.Infrastructure.Database.Configurations;

public class BlockTagConfiguration(bool isReadOnly = false) : IEntityTypeConfiguration<BlockTag>
{
	public void Configure(EntityTypeBuilder<BlockTag> builder)
	{
		builder.HasKey(x => x.Id);


		builder.HasOne(rel => rel.Tag)
			.WithMany()
			.HasForeignKey(rel => rel.TagId)
			.OnDelete(DeleteBehavior.SetNull);

		builder.HasOne(rel => rel.Block)
			.WithMany(e => e.RelationsToTags)
			.HasForeignKey(rel => rel.BlockId)
			.OnDelete(DeleteBehavior.Cascade);

		builder.HasKey(rel => new { rel.BlockId, rel.TagId });
	}
}
