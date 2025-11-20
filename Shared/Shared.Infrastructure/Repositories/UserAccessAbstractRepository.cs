using Datalake.Domain.ValueObjects;
using Datalake.Shared.Application.Interfaces.AccessRules;
using Datalake.Shared.Infrastructure.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Datalake.Shared.Infrastructure.Repositories;

public abstract class UserAccessAbstractRepository(IUserAccessDbContext context) : IUserAccessRepository
{
	public async Task<Dictionary<Guid, UserAccessValue>> GetAllAsync(CancellationToken ct = default)
	{
		var rules = await context.CalculatedAccessRules
			.AsNoTracking()
			.ToListAsync(ct);

		return BuildUserAccessDictionary(rules);
	}

	public async Task<UserAccessValue?> GetByUserGuidAsync(Guid userGuid, CancellationToken ct = default)
	{
		var rules = await context.CalculatedAccessRules
			.AsNoTracking()
			.Where(r => r.UserGuid == userGuid)
			.ToListAsync(ct);

		if (rules.Count == 0)
			return null;

		return BuildUserAccessValue(userGuid, rules);
	}

	public async Task<Dictionary<Guid, UserAccessValue>> GetMultipleAsync(IEnumerable<Guid> userGuids, CancellationToken ct = default)
	{
		var userGuidList = userGuids.ToList();
		if (userGuidList.Count == 0)
			return new Dictionary<Guid, UserAccessValue>();

		var rules = await context.CalculatedAccessRules
			.AsNoTracking()
			.Where(r => userGuidList.Contains(r.UserGuid))
			.ToListAsync(ct);

		return BuildUserAccessDictionary(rules);
	}

	private static Dictionary<Guid, UserAccessValue> BuildUserAccessDictionary(List<CalculatedAccessRule> rules)
	{
		return rules
			.GroupBy(r => r.UserGuid)
			.ToDictionary(
				g => g.Key,
				g => BuildUserAccessValue(g.Key, g.ToList())
			);
	}

	private static UserAccessValue BuildUserAccessValue(Guid userGuid, List<CalculatedAccessRule> rules)
	{
		UserAccessRuleValue? rootRule = null;
		var groupsRules = new Dictionary<Guid, UserAccessRuleValue>();
		var sourcesRules = new Dictionary<int, UserAccessRuleValue>();
		var blocksRules = new Dictionary<int, UserAccessRuleValue>();
		var tagsRules = new Dictionary<int, UserAccessRuleValue>();

		foreach (var rule in rules)
		{
			var ruleValue = new UserAccessRuleValue(rule.RuleId, rule.AccessType);

			if (rule.IsGlobal)
			{
				rootRule = ruleValue;
			}
			else if (rule.UserGroupGuid.HasValue)
			{
				groupsRules[rule.UserGroupGuid.Value] = ruleValue;
			}
			else if (rule.SourceId.HasValue)
			{
				sourcesRules[rule.SourceId.Value] = ruleValue;
			}
			else if (rule.BlockId.HasValue)
			{
				blocksRules[rule.BlockId.Value] = ruleValue;
			}
			else if (rule.TagId.HasValue)
			{
				tagsRules[rule.TagId.Value] = ruleValue;
			}
		}

		// Если глобального правила нет, то создаем правило с доступом None
		rootRule ??= UserAccessRuleValue.Empty;

		return new UserAccessValue(
			userGuid,
			rootRule,
			groupsRules,
			sourcesRules,
			blocksRules,
			tagsRules);
	}
}
