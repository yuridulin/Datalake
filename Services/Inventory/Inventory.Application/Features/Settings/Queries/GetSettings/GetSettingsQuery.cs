using Datalake.Inventory.Api.Models.Settings;
using Datalake.Shared.Application.Entities;
using Datalake.Shared.Application.Interfaces;

namespace Datalake.Inventory.Application.Features.Settings.Queries.GetSettings;

public record GetSettingsQuery(
	UserAccessValue User) : IQueryRequest<SettingsInfo>;
