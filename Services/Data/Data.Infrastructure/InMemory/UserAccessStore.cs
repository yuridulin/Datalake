using Datalake.Data.Application.Interfaces.Cache;
using Datalake.Shared.Application.Attributes;
using Datalake.Shared.Application.Entities;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;

namespace Datalake.Data.Infrastructure.InMemory;

[Singleton]
public class AccessStore(ILogger<AccessStore> logger) : IUserAccessStore
{
	private ConcurrentDictionary<Guid, UserAccessEntity> _local = [];
	private ConcurrentDictionary<Guid, UserAccessEntity> _mappingFromEnergoId = [];
	private readonly SemaphoreSlim _semaphore = new(1, 1);

	public UserAccessEntity? TryGet(Guid guid)
	{
		if (_local.TryGetValue(guid, out var userAuthInfo))
			return userAuthInfo;

		if (_mappingFromEnergoId.TryGetValue(guid, out userAuthInfo))
			return userAuthInfo;

		return null;
	}

	public async Task UpdateAsync(IEnumerable<UserAccessEntity> newAuthInfo)
	{
		await _semaphore.WaitAsync();

		try
		{
			var localUsers = new ConcurrentDictionary<Guid, UserAccessEntity>(newAuthInfo.ToDictionary(x => x.Guid));
			var mapping = new ConcurrentDictionary<Guid, UserAccessEntity>(newAuthInfo.Where(x => x.EnergoId.HasValue).ToDictionary(x => x.EnergoId!.Value));

			Interlocked.Exchange(ref _local, localUsers);
			Interlocked.Exchange(ref _mappingFromEnergoId, mapping);

			logger.LogInformation("Список прав доступа пользователей обновлен");
		}
		finally
		{
			_semaphore.Release();
		}
	}
}
