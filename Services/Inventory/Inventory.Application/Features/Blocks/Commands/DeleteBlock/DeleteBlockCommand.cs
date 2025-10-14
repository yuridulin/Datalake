using Datalake.Domain.ValueObjects;
using Datalake.Shared.Application.Interfaces;

namespace Datalake.Inventory.Application.Features.Blocks.Commands.DeleteBlock;

public record DeleteBlockCommand(
	UserAccessValue User,
	int BlockId) : ICommandRequest;
