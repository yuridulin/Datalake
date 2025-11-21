using Datalake.Domain.Entities;
using Datalake.Shared.Infrastructure.Database.Schema;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Datalake.Shared.Infrastructure.Database.Configurations;

public class TagInputConfiguration(DatabaseTableAccess access) : IEntityTypeConfiguration<TagInput>
{
	public void Configure(EntityTypeBuilder<TagInput> builder)
	{
		if (access == DatabaseTableAccess.Read)
			builder.ToView(InventorySchema.TagInputs.Name, InventorySchema.Name);
		else
			builder.ToTable(InventorySchema.TagInputs.Name, InventorySchema.Name);

		builder.HasKey(x => x.Id);

		if (access == DatabaseTableAccess.Write)
			builder.Property(x => x.Id).ValueGeneratedOnAdd();

		var relationToInput = builder.HasOne(input => input.InputTag)
			.WithMany(x => x.InputsUsingThisTag)
			.HasForeignKey(input => input.InputTagId);

		if (access == DatabaseTableAccess.Write)
			relationToInput.OnDelete(DeleteBehavior.SetNull);
	}
}
