using Datalake.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Datalake.Inventory.Infrastructure.Database.Configurations;

public class TagInputEntityConfiguration : IEntityTypeConfiguration<TagInputEntity>
{
	public void Configure(EntityTypeBuilder<TagInputEntity> builder)
	{
		builder.HasKey(x => x.Id);

		builder.HasOne(input => input.InputTag)
			.WithMany()
			.HasForeignKey(input => input.InputTagId)
			.OnDelete(DeleteBehavior.SetNull);
	}
}
