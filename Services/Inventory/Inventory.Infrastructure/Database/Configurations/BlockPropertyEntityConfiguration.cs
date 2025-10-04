using Datalake.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Datalake.Inventory.Infrastructure.Database.Configurations;

public class BlockPropertyEntityConfiguration : IEntityTypeConfiguration<BlockPropertyEntity>
{
	public void Configure(EntityTypeBuilder<BlockPropertyEntity> builder)
	{
		builder.HasKey(x => x.Id);
	}
}
