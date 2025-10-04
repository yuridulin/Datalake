using Datalake.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Datalake.Inventory.Infrastructure.Database.Configurations;

public class SourceEntityConfiguration : IEntityTypeConfiguration<SourceEntity>
{
	public void Configure(EntityTypeBuilder<SourceEntity> builder)
	{
		builder.HasKey(x => x.Id);
	}
}
