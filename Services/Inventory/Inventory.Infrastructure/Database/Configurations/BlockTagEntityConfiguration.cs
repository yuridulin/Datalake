using Datalake.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Datalake.Inventory.Infrastructure.Database.Configurations;

public class BlockTagEntityConfiguration : IEntityTypeConfiguration<BlockTagEntity>
{
	public void Configure(EntityTypeBuilder<BlockTagEntity> builder)
	{
		builder.HasKey(x => x.Id);
	}
}
