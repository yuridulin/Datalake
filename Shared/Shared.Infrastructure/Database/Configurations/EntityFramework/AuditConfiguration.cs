using Datalake.Domain.Entities;
using Datalake.Shared.Infrastructure.Database.Schema;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Datalake.Shared.Infrastructure.Database.Configurations.EntityFramework;

public class AuditConfiguration(DatabaseTableAccess access) : IEntityTypeConfiguration<AuditLog>
{
	public void Configure(EntityTypeBuilder<AuditLog> builder)
	{
		if (access == DatabaseTableAccess.Read)
			builder.ToView(InventorySchema.Logs.Name, InventorySchema.Name);
		else
			builder.ToTable(InventorySchema.Logs.Name, InventorySchema.Name);

		builder.HasKey(x => x.Id);

		if (access == DatabaseTableAccess.Write)
			builder.Property(x => x.Id).ValueGeneratedOnAdd();

		var relationToAuthors = builder.HasOne(x => x.Author)
			.WithMany(x => x.AuditActions)
			.HasForeignKey(x => x.AuthorGuid);

		var relationToSources = builder.HasOne(x => x.AffectedSource)
			.WithMany(x => x.AuditLogs)
			.HasForeignKey(x => x.AffectedSourceId);

		var relationToBlocks = builder.HasOne(x => x.AffectedBlock)
			.WithMany(x => x.AuditLogs)
			.HasForeignKey(x => x.AffectedBlockId);

		var relationToTags = builder.HasOne(x => x.AffectedTag)
			.WithMany(x => x.AuditLogs)
			.HasForeignKey(x => x.AffectedTagId);

		var relationToUsers = builder.HasOne(x => x.AffectedUser)
			.WithMany(x => x.AuditLogs)
			.HasForeignKey(x => x.AffectedUserGuid);

		var relationToUserGroups = builder.HasOne(x => x.AffectedUserGroup)
			.WithMany(x => x.AuditLogs)
			.HasForeignKey(x => x.AffectedUserGroupGuid);

		var relationToRules = builder.HasOne(x => x.AffectedAccessRights)
			.WithMany()
			.HasForeignKey(x => x.AffectedAccessRightsId);

		if (access == DatabaseTableAccess.Write)
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
