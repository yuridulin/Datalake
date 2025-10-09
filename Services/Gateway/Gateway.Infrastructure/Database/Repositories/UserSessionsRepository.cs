using Datalake.Domain.Entities;
using Datalake.Gateway.Application.Interfaces.Repositories;
using Datalake.Shared.Application.Attributes;
using Microsoft.EntityFrameworkCore;

namespace Datalake.Gateway.Infrastructure.Database.Repositories;

[Scoped]
public class UserSessionsRepository(GatewayDbContext context) : IUserSessionsRepository
{
	public async Task AddAsync(UserSession userSession, CancellationToken ct = default)
	{
		await context.UserSessions.AddAsync(userSession, ct);
	}

	public Task DeleteAsync(UserSession userSession, CancellationToken ct = default)
	{
		context.UserSessions.Remove(userSession);
		return Task.CompletedTask;
	}

	public async Task<IEnumerable<UserSession>> GetAllAsync(CancellationToken ct = default)
	{
		return await context.UserSessions.ToArrayAsync(ct);
	}

	public async Task<UserSession?> GetByGuidAsync(Guid guid, CancellationToken ct = default)
	{
		return await context.UserSessions.FirstOrDefaultAsync(x => x.UserGuid == guid, ct);
	}

	public async Task<UserSession?> GetByTokenAsync(string token, CancellationToken ct = default)
	{
		return await context.UserSessions.FirstOrDefaultAsync(x => x.Token.Value == token, ct);
	}

	public Task UpdateAsync(UserSession userSession, CancellationToken ct = default)
	{
		context.UserSessions.Update(userSession);
		return Task.CompletedTask;
	}
}
