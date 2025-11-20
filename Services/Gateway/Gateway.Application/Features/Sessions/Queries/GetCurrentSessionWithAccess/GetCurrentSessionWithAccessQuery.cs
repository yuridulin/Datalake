using Datalake.Gateway.Application.Models;
using Datalake.Shared.Application.Interfaces;

namespace Datalake.Gateway.Application.Features.Sessions.Queries.GetCurrentSessionWithAccess;

public record GetCurrentSessionWithAccessQuery : IQueryRequest<UserSessionWithAccessInfo>
{
	public required string Token { get; init; }
}
