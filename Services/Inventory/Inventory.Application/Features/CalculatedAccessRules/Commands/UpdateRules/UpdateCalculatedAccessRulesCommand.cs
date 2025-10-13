using Datalake.Domain.ValueObjects;
using Datalake.Shared.Application.Interfaces;

namespace Datalake.Inventory.Application.Features.CalculatedAccessRules.Commands.UpdateRules;

public record UpdateCalculatedAccessRulesCommand : ICommandRequest
{
	public required IEnumerable<CalculatedAccessRule> Rules { get; init; }
}
