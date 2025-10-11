using Datalake.Contracts.Public.Enums;

namespace Datalake.Shared.Application.Entities;

public interface IUserAccessEntity
{
	Guid? EnergoId { get; }
	Guid Guid { get; }

	AccessRuleValue RootRule { get; }
	Dictionary<int, AccessRuleValue> BlocksRules { get; }
	Dictionary<int, AccessRuleValue> SourcesRules { get; }
	Dictionary<int, AccessRuleValue> TagsRules { get; }
	Dictionary<Guid, AccessRuleValue> GroupsRules { get; }

	void AddBlockRule(int blockId, AccessRuleValue rule);
	void AddGroupRule(Guid groupGuid, AccessRuleValue rule);
	void AddSourceRule(int sourceId, AccessRuleValue rule);
	void AddTagRule(int tagId, AccessRuleValue rule);

	AccessRuleValue GetAccessToBlock(int blockId);
	AccessRuleValue GetAccessToSource(int sourceId);
	AccessRuleValue GetAccessToTag(int tagId);
	AccessRuleValue GetAccessToUserGroup(Guid groupGuid);

	bool HasAccessToBlock(AccessType minimalAccess, int blockId, bool withUnderlying = true);
	bool HasAccessToSource(AccessType minimalAccess, int sourceId, bool withUnderlying = true);
	bool HasAccessToTag(AccessType minimalAccess, int tagId, bool withUnderlying = true);
	bool HasAccessToUserGroup(AccessType minimalAccess, Guid groupGuid, bool withUnderlying = true);
	bool HasGlobalAccess(AccessType minimalAccess, bool withUnderlying = true);

	void ThrowIfNoAccessToBlock(AccessType minimalAccess, int blockId);
	void ThrowIfNoAccessToSource(AccessType minimalAccess, int sourceId);
	void ThrowIfNoAccessToTag(AccessType minimalAccess, int tagId);
	void ThrowIfNoAccessToUserGroup(AccessType minimalAccess, Guid groupGuid);
	void ThrowIfNoGlobalAccess(AccessType minimalAccess);
}