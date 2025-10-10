using Datalake.Domain.Entities;
using Datalake.Shared.Infrastructure.Schema;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using static Datalake.Shared.Infrastructure.ConfigurationsApplyHelper;

namespace Datalake.Shared.Infrastructure.Configurations;

public class TagInputConfiguration(TableAccess access) : IEntityTypeConfiguration<TagInput>
{
	public void Configure(EntityTypeBuilder<TagInput> builder)
	{
		if (access == TableAccess.Read)
			builder.ToView(InventorySchema.TagInputs.Name, InventorySchema.Name);
		else
			builder.ToTable(InventorySchema.TagInputs.Name, InventorySchema.Name);

		builder.HasKey(x => x.Id);

		if (access == TableAccess.Write)
			builder.Property(x => x.Id).ValueGeneratedOnAdd();

		var relationToInput = builder.HasOne(input => input.InputTag)
			.WithMany(x => x.InputsUsingThisTag)
			.HasForeignKey(input => input.InputTagId);

		if (access == TableAccess.Write)
			relationToInput.OnDelete(DeleteBehavior.SetNull);
	}
}
