using Datalake.Domain.Entities;
using Datalake.Shared.Infrastructure.Schema;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Datalake.Shared.Infrastructure.Configurations;

public class TagInputConfiguration(bool isReadOnly = false) : IEntityTypeConfiguration<TagInput>
{
	public void Configure(EntityTypeBuilder<TagInput> builder)
	{
		if (isReadOnly)
			builder.ToView(InventorySchema.TagInputs.Name, InventorySchema.Name);
		else
			builder.ToTable(InventorySchema.TagInputs.Name, InventorySchema.Name);

		builder.HasKey(x => x.Id);

		if (!isReadOnly)
			builder.Property(x => x.Id).ValueGeneratedOnAdd();

		var relationToInput = builder.HasOne(input => input.InputTag)
			.WithMany(x => x.InputsUsingThisTag)
			.HasForeignKey(input => input.InputTagId);

		if (!isReadOnly)
			relationToInput.OnDelete(DeleteBehavior.SetNull);
	}
}
