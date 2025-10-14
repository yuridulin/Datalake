using Datalake.Contracts.Public.Enums;
using Datalake.Inventory.Application.Interfaces.InMemory;
using Datalake.Shared.Application.Entities;
using Datalake.Shared.Application.Interfaces;

namespace Datalake.Inventory.Application.Features.AccessRules.Queries.GetCalculatedAccess;

public interface IGetCalculatedAccessHandler : IQueryHandler<GetCalculatedAccessQuery, IReadOnlyDictionary<Guid, UserAccessValue>> { }

public class GetCalculatedAccessHandler(IUserAccessCache userAccessCache) : IGetCalculatedAccessHandler, IQueryHandler<GetCalculatedAccessQuery, IReadOnlyDictionary<Guid, UserAccessValue>>
{
	public Task<IReadOnlyDictionary<Guid, UserAccessValue>> HandleAsync(GetCalculatedAccessQuery query, CancellationToken ct = default)
	{
		query.User.ThrowIfNoGlobalAccess(AccessType.Admin);

		var state = userAccessCache.State.UsersAccess;

		Dictionary<Guid, UserAccessValue> data = [];
		if (query.Guids != null)
		{
			var guids = query.Guids.ToArray();
			foreach (var guid in guids)
			{
				if (state.ContainsKey(guid))
					data.Add(guid, state[guid]);
			}
		}
		else
		{
			data = new(state);
		}

		return Task.FromResult<IReadOnlyDictionary<Guid, UserAccessValue>>(data);
	}
}
