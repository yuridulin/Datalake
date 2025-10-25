using Datalake.Shared.Infrastructure.Configurations;
using Microsoft.EntityFrameworkCore;

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
		modelBuilder.ApplyConfiguration(new CalculatedAccessRuleConfiguration(readOnlySettings.CalculatedAccessRules));
		modelBuilder.ApplyConfiguration(new EnergoIdConfiguration());
		modelBuilder.ApplyConfiguration(new SettingsConfiguration(readOnlySettings.Settings));
		modelBuilder.ApplyConfiguration(new SourceConfiguration(readOnlySettings.Sources));
		modelBuilder.ApplyConfiguration(new TagConfiguration(readOnlySettings.Tags));
		modelBuilder.ApplyConfiguration(new TagInputConfiguration(readOnlySettings.TagsInputs));
		modelBuilder.ApplyConfiguration(new TagThresholdConfiguration(readOnlySettings.TagsThresholds));
		modelBuilder.ApplyConfiguration(new TagValueConfiguration(readOnlySettings.TagsValues));
		modelBuilder.ApplyConfiguration(new UserConfiguration(readOnlySettings.Users));
		modelBuilder.ApplyConfiguration(new UserGroupConfiguration(readOnlySettings.UserGroups));
		modelBuilder.ApplyConfiguration(new UserGroupRelationConfiguration(readOnlySettings.UserGroupsRelations));
		modelBuilder.ApplyConfiguration(new UserSessionConfiguration(readOnlySettings.UserSessions));
	}

	public record ReadOnlySettings
	{
		public required TableAccess AccessRules { get; init; }
		public required TableAccess Audit { get; init; }
		public required TableAccess Blocks { get; init; }
		public required TableAccess BlocksProperties { get; init; }
		public required TableAccess BlocksTags { get; init; }
		public required TableAccess CalculatedAccessRules { get; init; }
		public required TableAccess Settings { get; init; }
		public required TableAccess Sources { get; init; }
		public required TableAccess Tags { get; init; }
		public required TableAccess TagsInputs { get; init; }
		public required TableAccess TagsThresholds { get; init; }
		public required TableAccess TagsValues { get; init; }
		public required TableAccess Users { get; init; }
		public required TableAccess UserGroups { get; init; }
		public required TableAccess UserGroupsRelations { get; init; }
		public required TableAccess UserSessions { get; init; }
	}

	public enum TableAccess
	{
		Read,
		Write,
	}
}
