using Datalake.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Datalake.Inventory.Infrastructure.Database.Configurations;

public class BlockPropertyConfiguration(bool isReadOnly = false) : IEntityTypeConfiguration<BlockProperty>
{
	public void Configure(EntityTypeBuilder<BlockProperty> builder)
	{
		builder.HasKey(x => x.Id);
	}
}
