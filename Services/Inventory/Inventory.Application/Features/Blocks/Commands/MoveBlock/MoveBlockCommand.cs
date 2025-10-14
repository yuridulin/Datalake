using Datalake.Shared.Application.Entities;
using Datalake.Shared.Application.Interfaces;

namespace Datalake.Inventory.Application.Features.Blocks.Commands.MoveBlock;

public record MoveBlockCommand(
	UserAccessValue User,
	int BlockId,
	int? ParentId) : ICommandRequest;
