using Datalake.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Datalake.Inventory.Infrastructure.Database.Configurations;

public class TagInputEntityConfiguration : IEntityTypeConfiguration<TagInput>
{
	public void Configure(EntityTypeBuilder<TagInput> builder)
	{
		builder.HasKey(x => x.Id);

		builder.HasOne(input => input.InputTag)
			.WithMany()
			.HasForeignKey(input => input.InputTagId)
			.OnDelete(DeleteBehavior.SetNull);
	}
}
