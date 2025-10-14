using Datalake.Shared.Application.Entities;
using Datalake.Shared.Application.Interfaces;

namespace Datalake.Inventory.Application.Features.AccessRules.Queries.GetCalculatedAccess;

public class GetCalculatedAccessQuery : IQueryRequest<IReadOnlyDictionary<Guid, UserAccessValue>>, IWithUserAccess
{
	public required UserAccessValue User { get; init; }
	public required IEnumerable<Guid>? Guids { get; init; }
}
