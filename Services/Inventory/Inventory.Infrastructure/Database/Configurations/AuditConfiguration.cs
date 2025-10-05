using Datalake.Domain.Entities;
using Datalake.Shared.Infrastructure.Schema;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Datalake.Inventory.Infrastructure.Database.Configurations;

public class AuditConfiguration(bool isReadOnly = false) : IEntityTypeConfiguration<Log>
{
	public void Configure(EntityTypeBuilder<Log> builder)
	{
		if (isReadOnly)
			builder.ToView(InventorySchema.Logs.Name, InventorySchema.Name);
		else
			builder.ToTable(InventorySchema.Logs.Name, InventorySchema.Name);

		builder.HasKey(x => x.Id);

		var relationToAuthors = builder.HasOne(x => x.Author)
			.WithMany()
			.HasForeignKey(x => x.AuthorGuid)
			.OnDelete(DeleteBehavior.SetNull);

		var relationToSources = builder.HasOne(x => x.AffectedSource)
			.WithMany()
			.HasForeignKey(x => x.AffectedSourceId)
			.OnDelete(DeleteBehavior.SetNull);

		var relationToBlocks = builder.HasOne(x => x.AffectedBlock)
			.WithMany()
			.HasForeignKey(x => x.AffectedBlockId)
			.OnDelete(DeleteBehavior.SetNull);

		var relationToTags = builder.HasOne(x => x.AffectedTag)
			.WithMany()
			.HasForeignKey(x => x.AffectedTagId)
			.OnDelete(DeleteBehavior.SetNull);

		var relationToUsers = builder.HasOne(x => x.AffectedUser)
			.WithMany()
			.HasForeignKey(x => x.AffectedUserGuid)
			.OnDelete(DeleteBehavior.SetNull);

		var relationToUserGroups = builder.HasOne(x => x.AffectedUserGroup)
			.WithMany()
			.HasForeignKey(x => x.AffectedUserGroupGuid)
			.OnDelete(DeleteBehavior.SetNull);

		var relationToRules = builder.HasOne(x => x.AffectedAccessRights)
			.WithMany()
			.HasForeignKey(x => x.AffectedAccessRightsId)
			.OnDelete(DeleteBehavior.SetNull);

		if (!isReadOnly)
		{
			relationToAuthors.OnDelete(DeleteBehavior.SetNull);
			relationToBlocks.OnDelete(DeleteBehavior.SetNull);
			relationToSources.OnDelete(DeleteBehavior.SetNull);
			relationToTags.OnDelete(DeleteBehavior.SetNull);
			relationToUsers.OnDelete(DeleteBehavior.SetNull);
			relationToUserGroups.OnDelete(DeleteBehavior.SetNull);
			relationToRules.OnDelete(DeleteBehavior.SetNull);
		}
	}
}
