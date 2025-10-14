using Datalake.Contracts.Public.Enums;
using Datalake.Domain.ValueObjects;
using Datalake.Inventory.Application.Features.CalculatedAccessRules.Queries.GetCalculatedAccessRulesInternal;
using Datalake.Shared.Application.Interfaces;

namespace Datalake.Inventory.Application.Features.CalculatedAccessRules.Queries.GetCalculatedAccessRules;

public interface IGetCalculatedAccessRulesHandler : IQueryHandler<GetCalculatedAccessRulesQuery, IReadOnlyDictionary<Guid, UserAccessValue>> { }

public class GetCalculatedAccessRulesHandler(
	IGetCalculatedAccessRulesInternalHandler calculatedAccessRulesInternalHandler) : IGetCalculatedAccessRulesHandler, IQueryHandler<GetCalculatedAccessRulesQuery, IReadOnlyDictionary<Guid, UserAccessValue>>
{
	public Task<IReadOnlyDictionary<Guid, UserAccessValue>> HandleAsync(GetCalculatedAccessRulesQuery query, CancellationToken ct = default)
	{
		query.User.ThrowIfNoGlobalAccess(AccessType.Admin);

		return calculatedAccessRulesInternalHandler.HandleAsync(new() { Guids = query.Guids }, ct);
	}
}
