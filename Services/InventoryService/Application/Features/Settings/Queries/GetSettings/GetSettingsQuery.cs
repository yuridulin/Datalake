using Datalake.InventoryService.Application.Interfaces;
using Datalake.PrivateApi.Entities;
using Datalake.PublicApi.Models.Settings;

namespace Datalake.InventoryService.Application.Features.Settings.Queries.GetSettings;

public record GetSettingsQuery(
	UserAccessEntity User) : IQueryRequest<SettingsInfo>;
