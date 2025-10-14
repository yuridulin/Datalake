using Datalake.Contracts.Internal.Protos;
using Datalake.Contracts.Public.Enums;
using Datalake.Data.Application.Interfaces;
using Datalake.Domain.ValueObjects;

namespace Datalake.Data.Host.Services;

public class GrpcInventoryApiClient(InventoryGrpcService.InventoryGrpcServiceClient client) : IInventoryApiClient
{
	public async Task<Dictionary<Guid, UserAccessValue>> GetCalculatedAccessAsync(
		IEnumerable<Guid> guids,
		CancellationToken ct = default)
	{
		var request = new GetCalculatedAccessRequest();
		request.UserGuids.AddRange(guids.Select(g => g.ToString()));

		var response = await client.GetCalculatedAccessAsync(request, cancellationToken: ct);

		// Маппинг из gRPC DTO → доменные объекты
		var dict = new Dictionary<Guid, UserAccessValue>();
		foreach (var ua in response.UserAccesses)
		{
			var guid = Guid.Parse(ua.UserGuid);
			dict[guid] = MapToDomain(ua);
		}

		return dict;
	}

	private static UserAccessValue MapToDomain(UserAccessResponse ua)
	{
		return new UserAccessValue(
			userGuid: Guid.Parse(ua.UserGuid),
			rootRule: new UserAccessRuleValue(ua.RootRule.Id, (AccessType)ua.RootRule.Access),
			blocksRules: ua.BlocksRules.ToDictionary(kv => kv.Key, kv => new UserAccessRuleValue(kv.Value.Id, (AccessType)kv.Value.Access)),
			sourcesRules: ua.SourcesRules.ToDictionary(kv => kv.Key, kv => new UserAccessRuleValue(kv.Value.Id, (AccessType)kv.Value.Access)),
			tagsRules: ua.TagsRules.ToDictionary(kv => kv.Key, kv => new UserAccessRuleValue(kv.Value.Id,(AccessType)kv.Value.Access)),
			groupsRules: ua.GroupsRules.ToDictionary(kv => Guid.Parse(kv.Key), kv => new UserAccessRuleValue(kv.Value.Id, (AccessType)kv.Value.Access))
		);
	}
}
