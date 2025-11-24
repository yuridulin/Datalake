using Datalake.Contracts.Models.Users;
using Datalake.Domain.Enums;
using Datalake.Inventory.Application.Exceptions;
using Datalake.Inventory.Application.Queries;
using Datalake.Shared.Application.Interfaces;

namespace Datalake.Inventory.Application.Features.Users.Queries.GetUserWithDetails;

public interface IGetUserWithDetailsHandler : IQueryHandler<GetUserWithDetailsQuery, UserWithGroupsInfo> { }

public class GetUserWithDetailsHandler(
	IUsersQueriesService usersQueriesService) : IGetUserWithDetailsHandler
{
	public async Task<UserWithGroupsInfo> HandleAsync(GetUserWithDetailsQuery query, CancellationToken ct = default)
	{
		query.User.ThrowIfNoGlobalAccess(AccessType.Manager);

		var user = await usersQueriesService.GetByGuidAsync(query.Guid, ct)
			?? throw InventoryNotFoundException.NotFoundUser(query.Guid);

		var userWithGroups = new UserWithGroupsInfo
		{
			Guid = user.Guid,
			AccessType = user.AccessType,
			FullName = user.FullName,
			Type = user.Type,
			Login = user.Login,
			Email = user.Email,
			UserGroups = await usersQueriesService.GetGroupsWithMemberAsync(user.Guid, ct),
		};

		return userWithGroups;
	}
}
