using Datalake.InventoryService.Application.Interfaces;
using Datalake.PrivateApi.Entities;

namespace Datalake.InventoryService.Application.Features.Settings.Commands.UpdateSettings;

public record UpdateSettingsCommand(
	UserAccessEntity User) : ICommandRequest;
