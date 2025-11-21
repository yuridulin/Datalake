using Datalake.Domain.Entities;
using Datalake.Shared.Infrastructure.Database.Schema;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Datalake.Shared.Infrastructure.Database.Configurations.EntityFramework;

public class UserGroupConfiguration(DatabaseTableAccess access) : IEntityTypeConfiguration<UserGroup>
{
	public void Configure(EntityTypeBuilder<UserGroup> builder)
	{
		if (access == DatabaseTableAccess.Read)
			builder.ToView(InventorySchema.UserGroups.Name, InventorySchema.Name);
		else
			builder.ToTable(InventorySchema.UserGroups.Name, InventorySchema.Name);

		builder.HasKey(x => x.Guid);

		// связь групп пользователей по иерархии
		var hierarchyRelations = builder.HasOne(group => group.Parent)
			.WithMany(group => group.Children)
			.HasForeignKey(group => group.ParentGuid);

		if (access == DatabaseTableAccess.Write)
			hierarchyRelations.OnDelete(DeleteBehavior.SetNull);

		// связь пользователей и групп пользователей
		builder.HasMany(group => group.Users)
			.WithMany(user => user.Groups)
			.UsingEntity<UserGroupRelation>(entity => entity.HasKey(x => x.Id));
	}
}
