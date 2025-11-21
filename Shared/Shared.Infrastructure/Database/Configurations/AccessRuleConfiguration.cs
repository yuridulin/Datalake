using Datalake.Domain.Entities;
using Datalake.Shared.Infrastructure.Database.Schema;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Datalake.Shared.Infrastructure.Database.Configurations;

public class AccessRuleConfiguration(DatabaseTableAccess access) : IEntityTypeConfiguration<AccessRule>
{
	public void Configure(EntityTypeBuilder<AccessRule> builder)
	{
		if (access == DatabaseTableAccess.Read)
			builder.ToView(InventorySchema.AccessRules.Name, InventorySchema.Name);
		else
			builder.ToTable(InventorySchema.AccessRules.Name, InventorySchema.Name);

		builder.HasKey(x => x.Id);

		// связи модели прав с объектами
		var relationToBlocks = builder.HasOne(x => x.Block)
			.WithMany(x => x.AccessRules)
			.HasForeignKey(x => x.BlockId);

		var relationToSources = builder.HasOne(x => x.Source)
			.WithMany(x => x.AccessRules)
			.HasForeignKey(x => x.SourceId);

		var relationToTags = builder.HasOne(x => x.Tag)
			.WithMany(x => x.AccessRules)
			.HasForeignKey(x => x.TagId);

		var relationToUsers = builder.HasOne(x => x.User)
			.WithMany(x => x.AccessRules)
			.HasForeignKey(x => x.UserGuid);

		var relationToUserGroups = builder.HasOne(x => x.UserGroup)
			.WithMany(x => x.AccessRules)
			.HasForeignKey(x => x.UserGroupGuid);

		if (access == DatabaseTableAccess.Write)
		{
			relationToBlocks.OnDelete(DeleteBehavior.SetNull);
			relationToSources.OnDelete(DeleteBehavior.SetNull);
			relationToTags.OnDelete(DeleteBehavior.SetNull);
			relationToUsers.OnDelete(DeleteBehavior.Cascade);
			relationToUserGroups.OnDelete(DeleteBehavior.Cascade);
		}
	}
}
