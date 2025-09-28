using Datalake.InventoryService.Application.Constants;
using Datalake.PrivateApi.Entities;
using System.Collections.Immutable;

namespace Datalake.InventoryService.Infrastructure.Cache.UserAccess;

public record class UserAccessState
{
	private ImmutableDictionary<Guid, UserAccessEntity> _accessByUserGuid;
	private ImmutableDictionary<Guid, UserAccessEntity> _accessByEnergoId;

	public UserAccessState()
	{
		_accessByUserGuid = ImmutableDictionary<Guid, UserAccessEntity>.Empty;
		_accessByEnergoId = ImmutableDictionary<Guid, UserAccessEntity>.Empty;
	}

	public UserAccessState(Dictionary<Guid, UserAccessEntity> userRights)
	{
		_accessByUserGuid = userRights.ToImmutableDictionary();
		_accessByEnergoId = _accessByUserGuid.Values
			.Where(x => x.EnergoId.HasValue)
			.ToImmutableDictionary(x => x.EnergoId!.Value, x => x);
	}

	public UserAccessEntity Get(Guid userIdOrEnergoId)
	{
		if (_accessByUserGuid.TryGetValue(userIdOrEnergoId, out var info))
			return info;
		else if (_accessByEnergoId.TryGetValue(userIdOrEnergoId, out info))
			return info;
		else
			throw Errors.NoAccess;
	}

	public bool TryGet(Guid userIdOrEnergoId, out UserAccessEntity info)
	{
		if (_accessByUserGuid.TryGetValue(userIdOrEnergoId, out info!))
			return true;
		else if (_accessByEnergoId.TryGetValue(userIdOrEnergoId, out info!))
			return true;
		else
			return false;
	}

	public Dictionary<Guid, UserAccessEntity> GetAll() => new(_accessByUserGuid);
}
