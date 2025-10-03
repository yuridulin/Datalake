using Datalake.Inventory.Application.Interfaces;
using Datalake.Shared.Application.Entities;

namespace Datalake.Inventory.Application.Features.AccessRules.Queries.GetCalculatedAccess;

public class GetCalculatedAccessQuery : IQueryRequest<IDictionary<Guid, UserAccessEntity>>, IWithUserAccess
{
	public required UserAccessEntity User { get; init; }
}
