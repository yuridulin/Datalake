using Datalake.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Datalake.Inventory.Infrastructure.Database.Configurations;

public class AuditEntityConfiguration : IEntityTypeConfiguration<AuditEntity>
{
	public void Configure(EntityTypeBuilder<AuditEntity> builder)
	{
		builder.HasKey(x => x.Id);

		builder.ToTable("Logs");

		builder.HasOne(x => x.Author)
			.WithMany(x => x.Actions)
			.HasForeignKey(x => x.AuthorGuid)
			.OnDelete(DeleteBehavior.SetNull);

		builder.HasOne(x => x.AffectedSource)
			.WithMany(x => x.Logs)
			.HasForeignKey(x => x.AffectedSourceId)
			.OnDelete(DeleteBehavior.SetNull);

		builder.HasOne(x => x.AffectedBlock)
			.WithMany(x => x.Logs)
			.HasForeignKey(x => x.AffectedBlockId)
			.OnDelete(DeleteBehavior.SetNull);

		builder.HasOne(x => x.AffectedTag)
			.WithMany(x => x.Logs)
			.HasForeignKey(x => x.AffectedTagId)
			.OnDelete(DeleteBehavior.SetNull);

		builder.HasOne(x => x.AffectedUser)
			.WithMany(x => x.Logs)
			.HasForeignKey(x => x.AffectedUserGuid)
			.OnDelete(DeleteBehavior.SetNull);

		builder.HasOne(x => x.AffectedUserGroup)
			.WithMany(x => x.Logs)
			.HasForeignKey(x => x.AffectedUserGroupGuid)
			.OnDelete(DeleteBehavior.SetNull);

		builder.HasOne(x => x.AffectedAccessRights)
			.WithMany(x => x.Logs)
			.HasForeignKey(x => x.AffectedAccessRightsId)
			.OnDelete(DeleteBehavior.SetNull);
	}
}
