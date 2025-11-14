using Datalake.Contracts.Models;
using Datalake.Domain.ValueObjects;
using Datalake.Gateway.Application.Interfaces;
using Datalake.Gateway.Application.Models;
using Datalake.Shared.Application.Interfaces;

namespace Datalake.Gateway.Application.Features.Queries.GetCurrentSessionWithAccess;

public interface IGetCurrentSessionWithAccessHandler : IQueryHandler<GetCurrentSessionWithAccessQuery, UserSessionWithAccessInfo> { }

public class GetCurrentSessionWithAccessHandler(
	ISessionsService sessionsService,
	IUserAccessService userAccessService) : IGetCurrentSessionWithAccessHandler
{
	public async Task<UserSessionWithAccessInfo> HandleAsync(GetCurrentSessionWithAccessQuery query, CancellationToken ct = default)
	{
		var sessionInfo = await sessionsService.GetAsync(query.Token, ct);
		var access = await userAccessService.AuthenticateAsync(sessionInfo.UserGuid, ct);

		var data = new UserSessionWithAccessInfo
		{
			Token = sessionInfo.Token,
			Type = sessionInfo.Type,
			UserGuid = sessionInfo.UserGuid,
			ExpirationTime = sessionInfo.ExpirationTime,
			Access = MapAccessEntityToInfo(access),
		};

		return data;
	}

	private static AccessInfo MapAccessEntityToInfo(UserAccessValue entity) => new()
	{
		RootRule = MapAccessRuleToInfo(entity.RootRule),
		Blocks = entity.BlocksRules.ToDictionary(x => x.Key, x => MapAccessRuleToInfo(x.Value)),
		Sources = entity.SourcesRules.ToDictionary(x => x.Key, x => MapAccessRuleToInfo(x.Value)),
		Tags = entity.TagsRules.ToDictionary(x => x.Key, x => MapAccessRuleToInfo(x.Value)),
		Groups = entity.GroupsRules.ToDictionary(x => x.Key, x => MapAccessRuleToInfo(x.Value)),
	};

	private static AccessRuleInfo MapAccessRuleToInfo(UserAccessRuleValue rule) => new(rule.Id, rule.Access);
}
