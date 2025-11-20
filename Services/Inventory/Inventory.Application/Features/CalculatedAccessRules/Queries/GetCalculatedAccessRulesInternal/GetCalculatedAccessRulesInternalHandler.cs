using Datalake.Domain.ValueObjects;
using Datalake.Inventory.Application.Interfaces;
using Datalake.Shared.Application.Interfaces;

namespace Datalake.Inventory.Application.Features.CalculatedAccessRules.Queries.GetCalculatedAccessRulesInternal;

public interface IGetCalculatedAccessRulesInternalHandler : IQueryHandler<GetCalculatedAccessRulesInternalQuery, IReadOnlyDictionary<Guid, UserAccessValue>> { }

public class GetCalculatedAccessRulesInternalHandler(IUsersAccessStore userAccessCache) : IGetCalculatedAccessRulesInternalHandler, IQueryHandler<GetCalculatedAccessRulesInternalQuery, IReadOnlyDictionary<Guid, UserAccessValue>>
{
	public Task<IReadOnlyDictionary<Guid, UserAccessValue>> HandleAsync(GetCalculatedAccessRulesInternalQuery query, CancellationToken ct = default)
	{
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
