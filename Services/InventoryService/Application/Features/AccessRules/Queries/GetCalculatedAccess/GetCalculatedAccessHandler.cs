using Datalake.InventoryService.Application.Interfaces;
using Datalake.InventoryService.Application.Interfaces.InMemory;
using Datalake.PrivateApi.Entities;

namespace Datalake.InventoryService.Application.Features.AccessRules.Queries.GetCalculatedAccess;

public interface IGetCalculatedAccessHandler : IQueryHandler<GetCalculatedAccessQuery, IDictionary<Guid, UserAccessEntity>> { }

public class GetCalculatedAccessHandler(IUserAccessCache userAccessCache) : IGetCalculatedAccessHandler
{
	public Task<IDictionary<Guid, UserAccessEntity>> HandleAsync(GetCalculatedAccessQuery query, CancellationToken ct = default)
	{
		query.User.ThrowIfNoGlobalAccess(PublicApi.Enums.AccessType.Admin);

		var data = userAccessCache.State.GetAll();

		return Task.FromResult<IDictionary<Guid, UserAccessEntity>>(data);
	}
}
