using Datalake.Domain.Entities;
using Datalake.Shared.Infrastructure.Schema;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using static Datalake.Shared.Infrastructure.ConfigurationsApplyHelper;

namespace Datalake.Shared.Infrastructure.Configurations;

public class SourceConfiguration(TableAccess access) : IEntityTypeConfiguration<Source>
{
	public void Configure(EntityTypeBuilder<Source> builder)
	{
		if (access == TableAccess.Read)
			builder.ToView(InventorySchema.Sources.Name, InventorySchema.Name);
		else
			builder.ToTable(InventorySchema.Sources.Name, InventorySchema.Name);

		builder.HasKey(x => x.Id);

		if (access == TableAccess.Write)
			builder.Property(x => x.Id).ValueGeneratedOnAdd();

		// связь источников и тегов
		var relationToTags = builder.HasMany(source => source.Tags)
			.WithOne(tag => tag.Source)
			.HasForeignKey(tag => tag.SourceId);

		if (access == TableAccess.Write)
			relationToTags.OnDelete(DeleteBehavior.SetNull);
	}
}
