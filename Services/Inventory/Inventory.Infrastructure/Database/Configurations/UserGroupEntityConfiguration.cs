using Datalake.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Datalake.Inventory.Infrastructure.Database.Configurations;

public class UserGroupEntityConfiguration : IEntityTypeConfiguration<UserGroupEntity>
{
	public void Configure(EntityTypeBuilder<UserGroupEntity> builder)
	{
		builder.HasKey(x => x.Guid);

		// связь групп пользователей по иерархии
		builder.HasOne(group => group.Parent)
			.WithMany(group => group.Children)
			.HasForeignKey(group => group.ParentGuid)
			.OnDelete(DeleteBehavior.SetNull);

		// связь пользователей и групп пользователей
		builder.HasMany(group => group.Users)
			.WithMany(user => user.Groups)
			.UsingEntity<UserGroupRelationEntity>(
				relation => relation
					.HasOne(rel => rel.User)
					.WithMany(user => user.GroupsRelations)
					.HasForeignKey(rel => rel.UserGuid)
					.OnDelete(DeleteBehavior.Cascade),
				relation => relation
					.HasOne(rel => rel.UserGroup)
					.WithMany(group => group.UsersRelations)
					.HasForeignKey(rel => rel.UserGroupGuid)
					.OnDelete(DeleteBehavior.Cascade),
				relation => relation
					.HasKey(rel => new { rel.UserGroupGuid, rel.UserGuid })
			);
	}
}

public class UserGroupRelationEntityConfiguration : IEntityTypeConfiguration<UserGroupRelationEntity>
{
	public void Configure(EntityTypeBuilder<UserGroupRelationEntity> builder)
	{
		builder.HasKey(x => x.Id);
	}
}
