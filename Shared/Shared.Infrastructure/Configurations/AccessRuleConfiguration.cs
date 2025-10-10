using Datalake.Domain.Entities;
using Datalake.Shared.Infrastructure.Schema;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using static Datalake.Shared.Infrastructure.ConfigurationsApplyHelper;

namespace Datalake.Shared.Infrastructure.Configurations;

public class AccessRuleConfiguration(TableAccess access) : IEntityTypeConfiguration<AccessRule>
{
	public void Configure(EntityTypeBuilder<AccessRule> builder)
	{
		if (access == TableAccess.Read)
			builder.ToView(InventorySchema.AccessRights.Name, InventorySchema.Name);
		else
			builder.ToTable(InventorySchema.AccessRights.Name, InventorySchema.Name);

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

		if (access == TableAccess.Write)
		{
			relationToBlocks.OnDelete(DeleteBehavior.SetNull);
			relationToSources.OnDelete(DeleteBehavior.SetNull);
			relationToTags.OnDelete(DeleteBehavior.SetNull);
			relationToUsers.OnDelete(DeleteBehavior.Cascade);
			relationToUserGroups.OnDelete(DeleteBehavior.Cascade);
		}
	}
}
