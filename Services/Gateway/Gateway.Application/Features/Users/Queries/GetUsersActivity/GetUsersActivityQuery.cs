using Datalake.Shared.Application.Interfaces;

namespace Datalake.Gateway.Application.Features.Users.Queries.GetUsersActivity;

public record GetUsersActivityQuery : IQueryRequest<Dictionary<Guid, DateTime?>>
{
	public required string Token { get; init; }

	public required IEnumerable<Guid> Users { get; init; }
}
