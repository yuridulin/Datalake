using Datalake.Contracts.Public.Enums;
using Datalake.Inventory.Application.Interfaces.InMemory;
using Datalake.Shared.Application.Entities;
using Datalake.Shared.Application.Interfaces;

namespace Datalake.Inventory.Application.Features.AccessRules.Queries.GetCalculatedAccess;

public interface IGetCalculatedAccessHandler : IQueryHandler<GetCalculatedAccessQuery, IReadOnlyDictionary<Guid, IUserAccessEntity>> { }

public class GetCalculatedAccessHandler(IUserAccessCache userAccessCache) : IGetCalculatedAccessHandler
{
	public Task<IReadOnlyDictionary<Guid, IUserAccessEntity>> HandleAsync(GetCalculatedAccessQuery query, CancellationToken ct = default)
	{
		query.User.ThrowIfNoGlobalAccess(AccessType.Admin);

		var data = userAccessCache.State.UsersAccess;

		return Task.FromResult(data);
	}
}
