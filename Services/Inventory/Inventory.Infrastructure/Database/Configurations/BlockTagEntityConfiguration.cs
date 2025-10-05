using Datalake.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Datalake.Inventory.Infrastructure.Database.Configurations;

public class BlockTagEntityConfiguration : IEntityTypeConfiguration<BlockTag>
{
	public void Configure(EntityTypeBuilder<BlockTag> builder)
	{
		builder.HasKey(x => x.Id);
	}
}
