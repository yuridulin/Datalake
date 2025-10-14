using Datalake.Domain.ValueObjects;
using Datalake.Shared.Application.Interfaces;

namespace Datalake.Inventory.Application.Features.CalculatedAccessRules.Queries.GetCalculatedAccessRules;

public class GetCalculatedAccessRulesQuery : IQueryRequest<IReadOnlyDictionary<Guid, UserAccessValue>>, IWithUserAccess
{
	public required UserAccessValue User { get; init; }

	public required IEnumerable<Guid>? Guids { get; init; }
}
