using Datalake.Domain.ValueObjects;
using Datalake.Shared.Infrastructure.Schema;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using static Datalake.Shared.Infrastructure.ConfigurationsApplyHelper;
using Table = Datalake.Shared.Infrastructure.Schema.InventorySchema.CalculatedAccessRules;

namespace Datalake.Shared.Infrastructure.Configurations;

public class CalculatedAccessRuleConfiguration(TableAccess access) : IEntityTypeConfiguration<CalculatedAccessRule>
{
	public void Configure(EntityTypeBuilder<CalculatedAccessRule> builder)
	{
		if (access == TableAccess.Read)
			builder.ToView(Table.Name, InventorySchema.Name);
		else
			builder.ToTable(Table.Name, InventorySchema.Name);

		builder.Property(x => x.Id).HasColumnName(Table.Columns.Id);
		builder.Property(x => x.UserGuid).HasColumnName(Table.Columns.UserGuid);
		builder.Property(x => x.AccessType).HasColumnName(Table.Columns.AccessType);
		builder.Property(x => x.IsGlobal).HasColumnName(Table.Columns.IsGlobal);
		builder.Property(x => x.TagId).HasColumnName(Table.Columns.TagId);
		builder.Property(x => x.BlockId).HasColumnName(Table.Columns.BlockId);
		builder.Property(x => x.SourceId).HasColumnName(Table.Columns.SourceId);
		builder.Property(x => x.UserGroupGuid).HasColumnName(Table.Columns.UserGroupGuid);
		builder.Property(x => x.RuleId).HasColumnName(Table.Columns.RuleId);
		builder.Property(x => x.UpdatedAt).HasColumnName(Table.Columns.UpdatedAt);

		builder.HasKey(x => x.Id);

		if (access == TableAccess.Write)
		{
			builder.Property(x => x.Id).ValueGeneratedOnAdd();
			builder.HasIndex(x => new { x.UserGuid, x.IsGlobal, x.BlockId, x.TagId, x.SourceId, x.UserGroupGuid }).IsUnique();
		}

		// связи модели прав с объектами
		var relationToUsers = builder.HasOne(x => x.User)
			.WithMany(x => x.CalculatedAccessRules)
			.HasForeignKey(x => x.UserGuid);

		var relationToBlocks = builder.HasOne(x => x.Block)
			.WithMany(x => x.CalculatedAccessRules)
			.HasForeignKey(x => x.BlockId);

		var relationToSources = builder.HasOne(x => x.Source)
			.WithMany(x => x.CalculatedAccessRules)
			.HasForeignKey(x => x.SourceId);

		var relationToTags = builder.HasOne(x => x.Tag)
			.WithMany(x => x.CalculatedAccessRules)
			.HasForeignKey(x => x.TagId);

		var relationToUserGroups = builder.HasOne(x => x.UserGroup)
			.WithMany(x => x.CalculatedAccessRules)
			.HasForeignKey(x => x.UserGroupGuid);

		var relationToRules = builder.HasOne(x => x.AccessRule)
			.WithMany(x => x.CalculatedAccessRules)
			.HasForeignKey(x => x.RuleId);

		if (access == TableAccess.Write)
		{
			relationToUsers.IsRequired().OnDelete(DeleteBehavior.Cascade);
			relationToBlocks.OnDelete(DeleteBehavior.Cascade);
			relationToSources.OnDelete(DeleteBehavior.Cascade);
			relationToTags.OnDelete(DeleteBehavior.Cascade);
			relationToUserGroups.OnDelete(DeleteBehavior.Cascade);
			relationToRules.IsRequired().OnDelete(DeleteBehavior.Cascade);
		}
	}
}
