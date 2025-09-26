using Datalake.DataService.Abstractions;
using Datalake.PrivateApi.Attributes;
using Datalake.PrivateApi.Entities;
using System.Collections.Concurrent;

namespace Datalake.DataService.Stores;

[Singleton]
public class AccessStore(ILogger<AccessStore> logger) : IAccessStore
{
	private ConcurrentDictionary<Guid, UserAccessEntity> _local = [];
	private ConcurrentDictionary<Guid, Guid> _mappingFromEnergoId = [];
	private readonly Lock _lock = new();

	public UserAccessEntity? TryGet(Guid guid)
	{
		if (_local.TryGetValue(guid, out var userAuthInfo))
			return userAuthInfo;

		if (_mappingFromEnergoId.TryGetValue(guid, out var energoId) && _local.TryGetValue(energoId, out userAuthInfo))
			return userAuthInfo;

		return null;
	}

	public void Update(IEnumerable<UserAccessEntity> newAuthInfo)
	{
		var localUsers = new ConcurrentDictionary<Guid, UserAccessEntity>(newAuthInfo.ToDictionary(x => x.Guid));
		var mapping = new ConcurrentDictionary<Guid, Guid>(newAuthInfo
			.Where(x => x.EnergoId.HasValue)
			.ToDictionary(x => x.EnergoId!.Value, x => x.Guid));

		lock (_lock)
		{
			_local = localUsers;
			_mappingFromEnergoId = mapping;
		}

		logger.LogInformation("Список прав доступа пользователей обновлен");
	}
}
