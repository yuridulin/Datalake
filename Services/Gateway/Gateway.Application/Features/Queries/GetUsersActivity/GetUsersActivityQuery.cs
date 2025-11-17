using Datalake.Shared.Application.Interfaces;

namespace Datalake.Gateway.Application.Features.Queries.GetUsersActivity;

public record GetUsersActivityQuery : IQueryRequest<IDictionary<Guid, DateTime?>>
{
	public required string Token { get; init; }

	public required IEnumerable<Guid> Users { get; init; }
}
