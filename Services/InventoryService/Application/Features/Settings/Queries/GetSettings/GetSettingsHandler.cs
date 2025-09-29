using Datalake.InventoryService.Application.Interfaces;
using Datalake.InventoryService.Domain.Queries;
using Datalake.PrivateApi.Exceptions;
using Datalake.PublicApi.Enums;
using Datalake.PublicApi.Models.Settings;

namespace Datalake.InventoryService.Application.Features.Settings.Queries.GetSettings;

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
