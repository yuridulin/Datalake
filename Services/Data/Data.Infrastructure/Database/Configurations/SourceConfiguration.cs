using Datalake.Domain.Entities;
using Datalake.Shared.Infrastructure.Schema;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Datalake.Data.Infrastructure.Database.Configurations;

public class SourceConfiguration : IEntityTypeConfiguration<Source>
{
	public void Configure(EntityTypeBuilder<Source> builder)
	{
		builder.ToView(InventorySchema.Sources.Name, InventorySchema.Name);

		builder.HasKey(x => x.Id);

		// связь источников и тегов
		builder.HasMany(source => source.Tags)
			.WithOne(tag => tag.Source)
			.HasForeignKey(tag => tag.SourceId);
	}
}
