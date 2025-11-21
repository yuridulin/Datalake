using Datalake.Domain.Entities;
using Datalake.Shared.Infrastructure.Database.Schema;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Datalake.Shared.Infrastructure.Database.Configurations;

public class SourceConfiguration(DatabaseTableAccess access) : IEntityTypeConfiguration<Source>
{
	public void Configure(EntityTypeBuilder<Source> builder)
	{
		if (access == DatabaseTableAccess.Read)
			builder.ToView(InventorySchema.Sources.Name, InventorySchema.Name);
		else
			builder.ToTable(InventorySchema.Sources.Name, InventorySchema.Name);

		builder.HasKey(x => x.Id);

		if (access == DatabaseTableAccess.Write)
			builder.Property(x => x.Id).ValueGeneratedOnAdd();

		// связь источников и тегов
		var relationToTags = builder.HasMany(source => source.Tags)
			.WithOne(tag => tag.Source)
			.HasForeignKey(tag => tag.SourceId);

		if (access == DatabaseTableAccess.Write)
			relationToTags.OnDelete(DeleteBehavior.SetNull);
	}
}
