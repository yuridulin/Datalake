using Datalake.Contracts.Models.Users;
using Datalake.Domain.Enums;
using Datalake.Inventory.Application.Queries;
using Datalake.Shared.Application.Interfaces;

namespace Datalake.Inventory.Application.Features.Users.Queries.GetUsers;

public interface IGetUsersHandler : IQueryHandler<GetUsersQuery, List<UserInfo>> { }

public class GetUsersHandler(
	IUsersQueriesService usersQueriesService) : IGetUsersHandler
{
	public async Task<List<UserInfo>> HandleAsync(GetUsersQuery query, CancellationToken ct = default)
	{
		query.User.ThrowIfNoGlobalAccess(AccessType.Manager);

		if (query.UserGuid.HasValue)
		{
			var info = await usersQueriesService.GetByGuidAsync(query.UserGuid.Value, ct)
				?? throw new ApplicationException($"Запрошенный пользователь не найден");

			return [info];
		}

		var data = await usersQueriesService.GetAsync(ct);
		return data;
	}
}
