using Datalake.Database.Constants;
using Datalake.PublicApi.Models.Auth;
using System.Collections.Immutable;

namespace Datalake.Inventory.InMemory.Models;

#pragma warning disable CS1591 // Отсутствует комментарий XML для открытого видимого типа или члена

public record class DatalakeAccessState
{
	private ImmutableDictionary<Guid, UserAuthInfo> _accessByUserGuid;
	private ImmutableDictionary<Guid, UserAuthInfo> _accessByEnergoId;

	public DatalakeAccessState()
	{
		_accessByUserGuid = ImmutableDictionary<Guid, UserAuthInfo>.Empty;
		_accessByEnergoId = ImmutableDictionary<Guid, UserAuthInfo>.Empty;
	}

	public DatalakeAccessState(Dictionary<Guid, UserAuthInfo> userRights)
	{
		_accessByUserGuid = userRights.ToImmutableDictionary();
		_accessByEnergoId = _accessByUserGuid.Values
			.Where(x => x.EnergoId.HasValue)
			.ToImmutableDictionary(x => x.EnergoId!.Value, x => x);
	}

	public UserAuthInfo Get(Guid userIdOrEnergoId)
	{
		if (_accessByUserGuid.TryGetValue(userIdOrEnergoId, out var info))
			return info;
		else if (_accessByEnergoId.TryGetValue(userIdOrEnergoId, out info))
			return info;
		else
			throw Errors.NoAccess;
	}

	public bool TryGet(Guid userIdOrEnergoId, out UserAuthInfo info)
	{
		if (_accessByUserGuid.TryGetValue(userIdOrEnergoId, out info!))
			return true;
		else if (_accessByEnergoId.TryGetValue(userIdOrEnergoId, out info!))
			return true;
		else
			return false;
	}

	public Dictionary<Guid, UserAuthInfo> GetAll() => new(_accessByUserGuid);
}

#pragma warning restore CS1591 // Отсутствует комментарий XML для открытого видимого типа или члена