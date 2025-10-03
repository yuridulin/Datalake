using Datalake.Data.Api.Models.Values;
using Datalake.Shared.Application.Entities;
using Datalake.Shared.Application.Interfaces;

namespace Datalake.Data.Application.Features.Values.Queries.GetValues;

public record GetValuesQuery : IQueryRequest<IEnumerable<ValuesResponse>>, IWithUserAccess
{
	public required UserAccessEntity User { get; init; }

	public required IEnumerable<ValuesRequest> Requests { get; init; }
}
