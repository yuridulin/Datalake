using Datalake.Domain.ValueObjects;
using Datalake.Inventory.Application.Interfaces.InMemory;
using Datalake.Inventory.Application.Models;
using System.Collections.Immutable;

namespace Datalake.Inventory.Infrastructure.InMemory.UserAccess;

public record class UserAccessCacheState : IUserAccessCacheState
{
	public long Version { get; private set; } = DateTime.MinValue.Ticks;

	public IReadOnlyDictionary<Guid, UserAccessValue> UsersAccess => _accessByUserGuid;

	public static UserAccessCacheState Empty => new();

	private UserAccessCacheState() { }

	private ImmutableDictionary<Guid, UserAccessValue> _accessByUserGuid = ImmutableDictionary<Guid, UserAccessValue>.Empty;

	public UserAccessCacheState(UsersAccessDto usersAccessDto)
	{
		_accessByUserGuid = usersAccessDto.UserAccessEntities.ToImmutableDictionary();
		Version = usersAccessDto.Version;
	}
}
