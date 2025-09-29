using Datalake.InventoryService.Application.Interfaces;

namespace Datalake.InventoryService.Application.Features.Settings.Commands.UpdateSettings;

public interface IUpdateSettingsHandler : ICommandHandler<UpdateSettingsCommand, bool> { }

public class UpdateSettingsHandler : IUpdateSettingsHandler
{
	public Task<bool> HandleAsync(UpdateSettingsCommand command, CancellationToken ct = default)
	{
		throw new NotImplementedException();
	}
}
