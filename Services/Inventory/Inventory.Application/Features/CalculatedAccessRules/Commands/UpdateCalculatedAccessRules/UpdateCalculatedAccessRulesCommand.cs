using Datalake.Domain.ValueObjects;
using Datalake.Shared.Application.Interfaces;

namespace Datalake.Inventory.Application.Features.CalculatedAccessRules.Commands.UpdateCalculatedAccessRules;

public record UpdateCalculatedAccessRulesCommand : ICommandRequest
{
	public required IEnumerable<CalculatedAccessRule> Rules { get; init; }
}
