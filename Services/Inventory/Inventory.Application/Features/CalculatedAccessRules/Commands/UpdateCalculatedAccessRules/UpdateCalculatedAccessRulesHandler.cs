using Datalake.Inventory.Application.Abstractions;
using Datalake.Inventory.Application.Interfaces.Persistent;
using Datalake.Inventory.Application.Repositories;
using Datalake.Shared.Application.Interfaces;
using Microsoft.Extensions.Logging;

namespace Datalake.Inventory.Application.Features.CalculatedAccessRules.Commands.UpdateCalculatedAccessRules;

public interface IUpdateCalculatedAccessRulesHandler : ICommandHandler<UpdateCalculatedAccessRulesCommand, bool> { }

public class UpdateCalculatedAccessRulesHandler(
	ICalculatedAccessRulesRepository calculatedAccessRulesRepository,
	IUnitOfWork unitOfWork,
	ILogger<UpdateCalculatedAccessRulesHandler> logger) :
		TransactionalCommandHandler<UpdateCalculatedAccessRulesCommand, bool>(unitOfWork, logger),
		IUpdateCalculatedAccessRulesHandler
{
	public override async Task<bool> ExecuteInTransactionAsync(UpdateCalculatedAccessRulesCommand command, CancellationToken ct = default)
	{
		await calculatedAccessRulesRepository.UpdateAsync(command.Rules, ct);

		return true;
	}
}
