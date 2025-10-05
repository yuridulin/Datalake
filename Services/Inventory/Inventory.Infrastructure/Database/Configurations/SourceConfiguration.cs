using Datalake.Domain.Entities;
using Datalake.Shared.Infrastructure.Schema;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Datalake.Inventory.Infrastructure.Database.Configurations;

public class SourceConfiguration(bool isReadOnly = false) : IEntityTypeConfiguration<Source>
{
	public void Configure(EntityTypeBuilder<Source> builder)
	{
		if (isReadOnly)
			builder.ToView(InventorySchema.Sources.Name, InventorySchema.Name);
		else
			builder.ToTable(InventorySchema.Sources.Name, InventorySchema.Name);

		builder.HasKey(x => x.Id);

		// связь источников и тегов
		var relationToTags = builder.HasMany(source => source.Tags)
			.WithOne(tag => tag.Source)
			.HasForeignKey(tag => tag.SourceId);

		if (!isReadOnly)
			relationToTags.OnDelete(DeleteBehavior.SetNull);
	}
}
