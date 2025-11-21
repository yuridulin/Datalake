namespace Datalake.Shared.Infrastructure.Database;

public record DatabaseTableAccessConfiguration
{
	public required DatabaseTableAccess AccessRules { get; init; }
	public required DatabaseTableAccess Audit { get; init; }
	public required DatabaseTableAccess Blocks { get; init; }
	public required DatabaseTableAccess BlocksProperties { get; init; }
	public required DatabaseTableAccess BlocksTags { get; init; }
	public required DatabaseTableAccess CalculatedAccessRules { get; init; }
	public required DatabaseTableAccess Settings { get; init; }
	public required DatabaseTableAccess Sources { get; init; }
	public required DatabaseTableAccess Tags { get; init; }
	public required DatabaseTableAccess TagsInputs { get; init; }
	public required DatabaseTableAccess TagsThresholds { get; init; }
	public required DatabaseTableAccess TagsValues { get; init; }
	public required DatabaseTableAccess Users { get; init; }
	public required DatabaseTableAccess UserGroups { get; init; }
	public required DatabaseTableAccess UserGroupsRelations { get; init; }
	public required DatabaseTableAccess UserSessions { get; init; }
}

public enum DatabaseTableAccess
{
	Read,
	Write,
}
