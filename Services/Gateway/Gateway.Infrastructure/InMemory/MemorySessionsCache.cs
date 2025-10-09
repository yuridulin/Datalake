using Datalake.Domain.Entities;
using Datalake.Gateway.Application.Interfaces;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;

namespace Datalake.Gateway.Infrastructure.InMemory;

public class MemorySessionsCache(IMemoryCache memoryCache) : ISessionsCache
{
	private static TimeSpan MemoryCacheDuration { get; } = TimeSpan.FromMinutes(5);

	public Task<UserSession?> GetAsync(string token)
	{
		if (memoryCache.TryGetValue<UserSession>(token, out var session))
			return Task.FromResult(session);

		return Task.FromResult<UserSession?>(null);
	}

	public Task RefreshAsync(UserSession session)
	{
		memoryCache.Remove(session.Token.Value);
		memoryCache.Set(session.Token.Value, session, MemoryCacheDuration);
		return Task.CompletedTask;
	}

	public Task RemoveAsync(string token)
	{
		memoryCache.Remove(token);
		return Task.CompletedTask;
	}

	public Task SetAsync(string token, UserSession session)
	{
		memoryCache.Set(token, session, MemoryCacheDuration);
		return Task.CompletedTask;
	}
}