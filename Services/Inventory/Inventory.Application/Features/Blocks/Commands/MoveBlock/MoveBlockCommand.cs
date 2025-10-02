using Datalake.Inventory.Application.Interfaces;
using Datalake.Shared.Domain.Entities;

namespace Datalake.Inventory.Application.Features.Blocks.Commands.MoveBlock;

public record MoveBlockCommand(
	UserAccessEntity User,
	int BlockId,
	int? ParentId) : ICommandRequest;
