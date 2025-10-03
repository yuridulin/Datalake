using Datalake.Contracts.Public.Enums;
using Datalake.Inventory.Application.Interfaces;
using Datalake.Inventory.Application.Interfaces.InMemory;
using Datalake.Shared.Application.Entities;

namespace Datalake.Inventory.Application.Features.AccessRules.Queries.GetCalculatedAccess;

public interface IGetCalculatedAccessHandler : IQueryHandler<GetCalculatedAccessQuery, IDictionary<Guid, UserAccessEntity>> { }

public class GetCalculatedAccessHandler(IUserAccessCache userAccessCache) : IGetCalculatedAccessHandler
{
	public Task<IDictionary<Guid, UserAccessEntity>> HandleAsync(GetCalculatedAccessQuery query, CancellationToken ct = default)
	{
		query.User.ThrowIfNoGlobalAccess(AccessType.Admin);

		var data = userAccessCache.State.GetAll();

		return Task.FromResult<IDictionary<Guid, UserAccessEntity>>(data);
	}
}
