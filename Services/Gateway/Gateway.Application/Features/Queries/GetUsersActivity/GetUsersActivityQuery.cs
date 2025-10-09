using Datalake.Shared.Application.Entities;
using Datalake.Shared.Application.Interfaces;

namespace Datalake.Gateway.Application.Features.Queries.GetUsersActivity;

public record GetUsersActivityQuery : IQueryRequest<IDictionary<Guid, DateTime?>>, IWithUserAccess
{
	public required UserAccessEntity User { get; init; }

	public required IEnumerable<Guid> Users { get; init; }
}
