using Datalake.Domain.Entities;
using Datalake.Shared.Infrastructure.Schema;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Datalake.Shared.Infrastructure.Configurations;

public class UserGroupRelationConfiguration(bool isReadOnly = false) : IEntityTypeConfiguration<UserGroupRelation>
{
	public void Configure(EntityTypeBuilder<UserGroupRelation> builder)
	{
		if (isReadOnly)
			builder.ToView(InventorySchema.UserGroupRelations.Name, InventorySchema.Name);
		else
			builder.ToTable(InventorySchema.UserGroupRelations.Name, InventorySchema.Name);

		builder.HasKey(x => x.Id);

		if (!isReadOnly)
			builder.Property(x => x.Id).ValueGeneratedOnAdd();

		var relationToUser = builder.HasOne(rel => rel.User)
			.WithMany(user => user.GroupsRelations)
			.HasForeignKey(rel => rel.UserGuid);

		var relationToGroup = builder.HasOne(rel => rel.UserGroup)
			.WithMany(group => group.UsersRelations)
			.HasForeignKey(rel => rel.UserGroupGuid);

		builder.HasIndex(rel => new { rel.UserGroupGuid, rel.UserGuid }).IsUnique();

		if (!isReadOnly)
		{
			relationToUser.OnDelete(DeleteBehavior.Cascade);
			relationToGroup.OnDelete(DeleteBehavior.Cascade);
		}
	}
}
