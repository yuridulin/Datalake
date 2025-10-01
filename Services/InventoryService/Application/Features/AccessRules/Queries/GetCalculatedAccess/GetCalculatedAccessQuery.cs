using Datalake.InventoryService.Application.Interfaces;
using Datalake.PrivateApi.Entities;

namespace Datalake.InventoryService.Application.Features.AccessRules.Queries.GetCalculatedAccess;

public class GetCalculatedAccessQuery : IQueryRequest<IDictionary<Guid, UserAccessEntity>>, IWithUserAccess
{
	public required UserAccessEntity User { get; init; }
}
