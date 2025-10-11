using Datalake.Shared.Application.Entities;
using Datalake.Shared.Application.Interfaces;

namespace Datalake.Inventory.Application.Features.AccessRules.Queries.GetCalculatedAccess;

public class GetCalculatedAccessQuery : IQueryRequest<IReadOnlyDictionary<Guid, IUserAccessEntity>>, IWithUserAccess
{
	public required UserAccessEntity User { get; init; }
}
