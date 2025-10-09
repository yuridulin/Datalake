using Datalake.Gateway.Application.Models;
using Datalake.Shared.Application.Interfaces;

namespace Datalake.Gateway.Application.Features.Queries.GetCurrentSessionWithAccess;

public record GetCurrentSessionWithAccessQuery : IQueryRequest<UserSessionWithAccessInfo>
{
	public required string Token { get; init; }
}
