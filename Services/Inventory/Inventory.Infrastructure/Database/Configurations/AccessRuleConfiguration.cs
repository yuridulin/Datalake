using Datalake.Domain.Entities;
using Datalake.Shared.Infrastructure.Schema;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Datalake.Inventory.Infrastructure.Database.Configurations;

public class AccessRuleConfiguration(bool isReadOnly = false) : IEntityTypeConfiguration<AccessRights>
{
	public void Configure(EntityTypeBuilder<AccessRights> builder)
	{
		if (isReadOnly)
			builder.ToView(InventorySchema.AccessRights.Name, InventorySchema.Name);
		else
			builder.ToTable(InventorySchema.AccessRights.Name, InventorySchema.Name);

		builder.HasKey(x => x.Id);

		// связи модели прав с объектами
		var relationToBlocks = builder.HasOne(x => x.Block)
			.WithMany()
			.HasForeignKey(x => x.BlockId);

		var relationToSources = builder.HasOne(x => x.Source)
			.WithMany()
			.HasForeignKey(x => x.SourceId);

		var relationToTags = builder.HasOne(x => x.Tag)
			.WithMany()
			.HasForeignKey(x => x.TagId);

		var relationToUsers = builder.HasOne(x => x.User)
			.WithMany()
			.HasForeignKey(x => x.UserGuid);

		var relationToUserGroups = builder.HasOne(x => x.UserGroup)
			.WithMany()
			.HasForeignKey(x => x.UserGroupGuid);

		if (!isReadOnly)
		{
			relationToBlocks.OnDelete(DeleteBehavior.SetNull);
			relationToSources.OnDelete(DeleteBehavior.SetNull);
			relationToTags.OnDelete(DeleteBehavior.SetNull);
			relationToUsers.OnDelete(DeleteBehavior.Cascade);
			relationToUserGroups.OnDelete(DeleteBehavior.Cascade);
		}
	}
}
