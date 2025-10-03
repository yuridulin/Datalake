using Datalake.Inventory.Application.Interfaces;
using Datalake.Shared.Application.Entities;

namespace Datalake.Inventory.Application.Features.Blocks.Commands.DeleteBlock;

public record DeleteBlockCommand(
	UserAccessEntity User,
	int BlockId) : ICommandRequest;
