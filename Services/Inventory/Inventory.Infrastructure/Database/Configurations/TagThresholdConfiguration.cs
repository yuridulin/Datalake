using Datalake.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Datalake.Inventory.Infrastructure.Database.Configurations;

public class TagThresholdConfiguration : IEntityTypeConfiguration<TagThreshold>
{
	public void Configure(EntityTypeBuilder<TagThreshold> builder)
	{
		builder.HasKey(x => x.Id);
	}
}
