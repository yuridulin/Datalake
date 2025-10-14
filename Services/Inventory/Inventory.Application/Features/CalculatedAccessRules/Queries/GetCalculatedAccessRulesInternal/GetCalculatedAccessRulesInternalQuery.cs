using Datalake.Domain.ValueObjects;
using Datalake.Shared.Application.Interfaces;

namespace Datalake.Inventory.Application.Features.CalculatedAccessRules.Queries.GetCalculatedAccessRulesInternal;

public class GetCalculatedAccessRulesInternalQuery : IQueryRequest<IReadOnlyDictionary<Guid, UserAccessValue>>
{
	public required IEnumerable<Guid>? Guids { get; init; }
}
