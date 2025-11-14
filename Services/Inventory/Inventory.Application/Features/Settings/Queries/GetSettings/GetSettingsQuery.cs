using Datalake.Contracts.Models.Settings;
using Datalake.Domain.ValueObjects;
using Datalake.Shared.Application.Interfaces;

namespace Datalake.Inventory.Application.Features.Settings.Queries.GetSettings;

public record GetSettingsQuery(
	UserAccessValue User) : IQueryRequest<SettingsInfo>;
