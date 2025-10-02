using Datalake.InventoryService.Application.Abstractions;
using Datalake.InventoryService.Application.Interfaces;
using Datalake.InventoryService.Application.Interfaces.Persistent;
using Datalake.InventoryService.Application.Repositories;
using Datalake.InventoryService.Domain.Entities;
using Datalake.PublicApi.Enums;

namespace Datalake.InventoryService.Application.Features.Settings.Commands.UpdateSettings;

public interface IUpdateSettingsHandler : ICommandHandler<UpdateSettingsCommand, bool> { }

public class UpdateSettingsHandler(
	ISettingsRepository settingsRepository,
	IAuditRepository auditRepository,
	IUnitOfWork unitOfWork,
	ILogger<UpdateSettingsHandler> logger) : TransactionalCommandHandler<UpdateSettingsCommand, bool>(unitOfWork, logger), IUpdateSettingsHandler
{
	public override void CheckPermissions(UpdateSettingsCommand command)
	{
		command.User.ThrowIfNoGlobalAccess(AccessType.Admin);
	}

	public override async Task<bool> ExecuteInTransactionAsync(UpdateSettingsCommand command, CancellationToken ct = default)
	{
		var settings = await settingsRepository.GetAsync(ct);

		if (settings == null)
		{
			settings = new SettingsEntity(
				keycloakHost: command.KeycloakHost,
				keycloakClient: command.KeycloakClient,
				energoIdApi: command.EnergoIdApi,
				instanceName: command.InstanceName);
			await settingsRepository.AddAsync(settings, ct);
		}
		else
		{
			settings.Update(
				keycloakHost: command.KeycloakHost,
				keycloakClient: command.KeycloakClient,
				energoIdApi: command.EnergoIdApi,
				instanceName: command.InstanceName);
			await settingsRepository.UpdateAsync(settings, ct);
		}

		var audit = new AuditEntity(command.User.Guid, $"Настройки изменены");
		await auditRepository.AddAsync(audit, ct);

		return true;
	}
}
