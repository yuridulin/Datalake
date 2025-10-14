using Datalake.Data.Application.Interfaces.Cache;
using Datalake.Domain.ValueObjects;
using Datalake.Shared.Application.Attributes;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;

namespace Datalake.Data.Infrastructure.InMemory;

[Singleton]
public class UserAccessStore(ILogger<UserAccessStore> logger) : IUserAccessStore
{
	private ConcurrentDictionary<Guid, UserAccessValue> _state = [];
	private readonly SemaphoreSlim _semaphore = new(1, 1);

	public UserAccessValue? TryGet(Guid guid)
	{
		if (_state.TryGetValue(guid, out var userAuthInfo))
			return userAuthInfo;

		return null;
	}

	public async Task UpdateAsync(IDictionary<Guid, UserAccessValue> usersAccess)
	{
		await _semaphore.WaitAsync();

		try
		{
			foreach (var user in usersAccess)
			{
				_state.AddOrUpdate(user.Key, user.Value, (key, previous) => user.Value);
			}

			logger.LogInformation("Список прав доступа пользователей обновлен");
		}
		finally
		{
			_semaphore.Release();
		}
	}
}
