using Datalake.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Datalake.Inventory.Infrastructure.Database.Configurations;

public class UserGroupRelationEntityConfiguration : IEntityTypeConfiguration<UserGroupRelation>
{
	public void Configure(EntityTypeBuilder<UserGroupRelation> builder)
	{
		builder.HasKey(x => x.Id);
		builder.Property(x => x.Id).ValueGeneratedOnAdd();
	}
}
