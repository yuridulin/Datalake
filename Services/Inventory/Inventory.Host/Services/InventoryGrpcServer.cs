using Datalake.Contracts.Internal.Protos;
using Datalake.Domain.ValueObjects;
using Datalake.Inventory.Application.Features.CalculatedAccessRules.Queries.GetCalculatedAccessRulesInternal;
using Grpc.Core;

namespace Datalake.Inventory.Host.Services;

public class InventoryGrpcServer(IGetCalculatedAccessRulesInternalHandler handler) : InventoryGrpcService.InventoryGrpcServiceBase
{
	public override async Task<GetCalculatedAccessResponse> GetCalculatedAccess(GetCalculatedAccessRequest request, ServerCallContext context)
	{
		var guids = request.UserGuids
			.Select(x => Guid.TryParse(x, out var g) ? g : Guid.Empty)
			.Where(g => g != Guid.Empty)
			.ToArray();

		var data = await handler.HandleAsync(new() { Guids = guids });

		var response = new GetCalculatedAccessResponse();
		foreach (var kvp in data)
		{
			response.UserAccesses.Add(MapUserAccess(kvp.Key, kvp.Value));
		}

		return response;
	}

	public override async Task GetCalculatedAccessStream(GetCalculatedAccessRequest request, IServerStreamWriter<GetCalculatedAccessResponse> responseStream, ServerCallContext context)
	{
		var guids = request.UserGuids
			.Select(x => Guid.TryParse(x, out var g) ? g : Guid.Empty)
			.Where(g => g != Guid.Empty)
			.ToArray();

		var data = await handler.HandleAsync(new() { Guids = guids });

		foreach (var kvp in data)
		{
			var singleResponse = new GetCalculatedAccessResponse();
			singleResponse.UserAccesses.Add(MapUserAccess(kvp.Key, kvp.Value));

			await responseStream.WriteAsync(singleResponse);
		}
	}

	private static UserAccessResponse MapUserAccess(Guid guid, UserAccessValue value)
	{
		var resp = new UserAccessResponse
		{
			UserGuid = guid.ToString(),
			RootRule = new AccessRule
			{
				Id = value.RootRule.Id,
				Access = (int)value.RootRule.Access
			}
		};

		foreach (var kv in value.BlocksRules)
		{
			resp.BlocksRules.Add(kv.Key, new AccessRule { Id = kv.Value.Id, Access = (int)kv.Value.Access });
		}
		foreach (var kv in value.SourcesRules)
		{
			resp.SourcesRules.Add(kv.Key, new AccessRule { Id = kv.Value.Id, Access = (int)kv.Value.Access });
		}
		foreach (var kv in value.TagsRules)
		{
			resp.TagsRules.Add(kv.Key, new AccessRule { Id = kv.Value.Id, Access = (int)kv.Value.Access });
		}
		foreach (var kv in value.GroupsRules)
		{
			resp.GroupsRules.Add(kv.Key.ToString(), new AccessRule { Id = kv.Value.Id, Access = (int)kv.Value.Access });
		}

		return resp;
	}
}
