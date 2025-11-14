using Datalake.Contracts.Models.Settings;
using Datalake.Domain.Enums;
using Datalake.Domain.Exceptions;
using Datalake.Inventory.Application.Queries;
using Datalake.Shared.Application.Interfaces;

namespace Datalake.Inventory.Application.Features.Settings.Queries.GetSettings;

public interface IGetSettingsHandler : IQueryHandler<GetSettingsQuery, SettingsInfo> { }

public class GetSettingsHandler(
	ISettingsQueriesService settingsQueriesService) : IGetSettingsHandler
{
	public async Task<SettingsInfo> HandleAsync(GetSettingsQuery query, CancellationToken ct = default)
	{
		query.User.ThrowIfNoGlobalAccess(AccessType.Admin);

		var data = await settingsQueriesService.GetAsync(ct)
			?? throw new DomainException("NO_SETTINGS", "Запись настроек не была создана! Необходимо ее создать");

		return data;
	}
}
