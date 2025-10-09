using Datalake.Domain.Entities;
using Datalake.Gateway.Application.Interfaces.Repositories;
using Datalake.Shared.Application.Attributes;
using Microsoft.EntityFrameworkCore;

namespace Datalake.Gateway.Infrastructure.Database.Repositories;

[Scoped]
public class UsersRepository(GatewayDbContext context) : IUsersRepository
{
	public async Task<User?> GetByEnergoIdAsync(Guid guid, CancellationToken ct = default)
	{
		return await context.Users.AsNoTracking().FirstOrDefaultAsync(x => x.Guid == guid, ct);
	}

	public async Task<User?> GetByLoginAsync(string login, CancellationToken ct = default)
	{
		return await context.Users.AsNoTracking().FirstOrDefaultAsync(x => x.Login == login, ct);
	}
}
