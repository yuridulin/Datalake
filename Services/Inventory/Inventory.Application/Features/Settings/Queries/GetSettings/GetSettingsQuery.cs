using Datalake.Inventory.Application.Interfaces;
using Datalake.Inventory.Api.Models.Settings;
using Datalake.Shared.Application.Entities;

namespace Datalake.Inventory.Application.Features.Settings.Queries.GetSettings;

public record GetSettingsQuery(
	UserAccessEntity User) : IQueryRequest<SettingsInfo>;
