using Datalake.Contracts.Models.Data.Values;
using Datalake.Domain.ValueObjects;
using Datalake.Shared.Application.Interfaces;

namespace Datalake.Data.Application.Features.Values.Queries.GetValues;

public record GetValuesQuery : IQueryRequest<IEnumerable<ValuesResponse>>, IWithUserAccess
{
	public required UserAccessValue User { get; init; }

	public required IEnumerable<ValuesRequest> Requests { get; init; }
}
