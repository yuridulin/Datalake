using Datalake.Inventory.Application.Interfaces;
using Datalake.Shared.Application.Entities;

namespace Datalake.Inventory.Application.Features.Settings.Commands.UpdateSettings;

public record UpdateSettingsCommand(
	UserAccessEntity User,
	string KeycloakHost,
	string KeycloakClient,
	string EnergoIdApi,
	string InstanceName) : ICommandRequest;
