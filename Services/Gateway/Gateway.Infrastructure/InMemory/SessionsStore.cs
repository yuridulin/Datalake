using Datalake.Domain.Entities;
using Datalake.Gateway.Application.Interfaces.Storage;
using System.Collections.Concurrent;

namespace Datalake.Gateway.Infrastructure.InMemory;

public class SessionsStore : ISessionsStore
{
	private readonly ConcurrentDictionary<string, UserSession> store = [];

	public UserSession? Get(string token)
	{
		if (store.TryGetValue(token, out var session))
			return session;

		return null;
	}

	public void Remove(string token)
	{
		store.TryRemove(token, out _);
	}

	public void Set(string token, UserSession session)
	{
		store.AddOrUpdate(token, _ => session, (_, _) => session);
	}
}