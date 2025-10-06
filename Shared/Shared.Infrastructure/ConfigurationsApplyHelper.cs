using Datalake.Shared.Infrastructure.Configurations;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Datalake.Shared.Infrastructure;

public static class ConfigurationsApplyHelper
{
	public static void ApplyConfigurations(this ModelBuilder modelBuilder, ReadOnlySettings readOnlySettings)
	{
		modelBuilder.ApplyConfiguration(new AccessRuleConfiguration(readOnlySettings.AccessRules));
		modelBuilder.ApplyConfiguration(new AuditConfiguration(readOnlySettings.Audit));
		modelBuilder.ApplyConfiguration(new BlockConfiguration(readOnlySettings.Blocks));
		modelBuilder.ApplyConfiguration(new BlockPropertyConfiguration(readOnlySettings.BlocksProperties));
		modelBuilder.ApplyConfiguration(new BlockTagConfiguration(readOnlySettings.BlocksTags));
		modelBuilder.ApplyConfiguration(new EnergoIdConfiguration());
		modelBuilder.ApplyConfiguration(new SettingsConfiguration(readOnlySettings.Settings));
		modelBuilder.ApplyConfiguration(new SourceConfiguration(readOnlySettings.Sources));
		modelBuilder.ApplyConfiguration(new TagConfiguration(readOnlySettings.Tags));
		modelBuilder.ApplyConfiguration(new TagInputConfiguration(readOnlySettings.TagsInputs));
		modelBuilder.ApplyConfiguration(new TagThresholdConfiguration(readOnlySettings.TagsThresholds));
		modelBuilder.ApplyConfiguration(new TagHistoryConfiguration(readOnlySettings.TagsHistory));
		modelBuilder.ApplyConfiguration(new UserConfiguration(readOnlySettings.Users));
		modelBuilder.ApplyConfiguration(new UserGroupConfiguration(readOnlySettings.UserGroups));
		modelBuilder.ApplyConfiguration(new UserGroupRelationConfiguration(readOnlySettings.UserGroupsRelations));
	}

	public record ReadOnlySettings
	{
		public required bool AccessRules { get; init; }
		public required bool Audit { get; init; }
		public required bool Blocks { get; init; }
		public required bool BlocksProperties { get; init; }
		public required bool BlocksTags { get; init; }
		public required bool Settings { get; init; }
		public required bool Sources { get; init; }
		public required bool Tags { get; init; }
		public required bool TagsInputs { get; init; }
		public required bool TagsThresholds { get; init; }
		public required bool TagsHistory { get; init; }
		public required bool Users { get; init; }
		public required bool UserGroups { get; init; }
		public required bool UserGroupsRelations { get; init; }
	}
}
