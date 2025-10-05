using Datalake.Domain.Entities;
using Datalake.Shared.Infrastructure.Schema;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Datalake.Data.Infrastructure.Database.Configurations;

public class TagThresholdConfiguration : IEntityTypeConfiguration<TagThreshold>
{
	public void Configure(EntityTypeBuilder<TagThreshold> builder)
	{
		builder.ToView(InventorySchema.TagThresholds.Name, InventorySchema.Name);

		builder.HasKey(x => x.Id);
	}
}
