using Datalake.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Datalake.Inventory.Infrastructure.Database.Configurations;

public class AccessRuleEntityConfiguration : IEntityTypeConfiguration<AccessRights>
{
	public void Configure(EntityTypeBuilder<AccessRights> builder)
	{
		builder.HasKey(x => x.Id);

		// связи модели прав с объектами
		builder.HasOne(x => x.Block)
			.WithMany(x => x.AccessRules)
			.HasForeignKey(x => x.BlockId)
			.OnDelete(DeleteBehavior.SetNull);

		builder.HasOne(x => x.Source)
			.WithMany(x => x.AccessRules)
			.HasForeignKey(x => x.SourceId)
			.OnDelete(DeleteBehavior.SetNull);

		builder.HasOne(x => x.Tag)
			.WithMany(x => x.AccessRules)
			.HasForeignKey(x => x.TagId)
			.OnDelete(DeleteBehavior.SetNull);

		builder.HasOne(x => x.User)
			.WithMany(x => x.AccessRules)
			.HasForeignKey(x => x.UserGuid)
			.OnDelete(DeleteBehavior.Cascade);

		builder.HasOne(x => x.UserGroup)
			.WithMany(x => x.AccessRules)
			.HasForeignKey(x => x.UserGroupGuid)
			.OnDelete(DeleteBehavior.Cascade);
	}
}
