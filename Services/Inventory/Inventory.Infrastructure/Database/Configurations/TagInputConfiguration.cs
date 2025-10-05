using Datalake.Domain.Entities;
using Datalake.Shared.Infrastructure.Schema;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Datalake.Inventory.Infrastructure.Database.Configurations;

public class TagInputConfiguration(bool isReadOnly = false) : IEntityTypeConfiguration<TagInput>
{
	public void Configure(EntityTypeBuilder<TagInput> builder)
	{
		builder.ToTable(InventorySchema.TagInputs.Name, InventorySchema.Name);

		builder.HasKey(x => x.Id);

		builder.HasOne(input => input.InputTag)
			.WithMany()
			.HasForeignKey(input => input.InputTagId)
			.OnDelete(DeleteBehavior.SetNull);
	}
}
