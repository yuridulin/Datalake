namespace Datalake.Shared.Infrastructure.Schema;

public static class InventorySchema
{
	public static string Name { get; } = "public";

	public static class AccessRights
	{
		public static string Name { get; } = nameof(AccessRights);
	}

	public static class BlockProperties
	{
		public static string Name { get; } = nameof(BlockProperties);
	}

	public static class BlockTags
	{
		public static string Name { get; } = nameof(BlockTags);
	}

	public static class Blocks
	{
		public static string Name { get; } = nameof(Blocks);
	}

	public static class Logs
	{
		public static string Name { get; } = nameof(Logs);
	}

	public static class Settings
	{
		public static string Name { get; } = nameof(Settings);
	}

	public static class Sources
	{
		public static string Name { get; } = nameof(Sources);
	}

	public static class Tags
	{
		public static string Name { get; } = nameof(Tags);
	}

	public static class TagInputs
	{
		public static string Name { get; } = nameof(TagInputs);
	}

	public static class TagThresholds
	{
		public static string Name { get; } = nameof(TagThresholds);
	}

	public static class UserGroupRelations
	{
		public static string Name { get; } = nameof(UserGroupRelations);
	}

	public static class UserGroups
	{
		public static string Name { get; } = nameof(UserGroups);
	}

	public static class Users
	{
		public static string Name { get; } = nameof(Users);
	}
}
