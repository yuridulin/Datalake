using Datalake.Shared.Application.Entities;
using Datalake.Shared.Application.Interfaces;

namespace Datalake.Inventory.Application.Features.Blocks.Commands.DeleteBlock;

public record DeleteBlockCommand(
	UserAccessEntity User,
	int BlockId) : ICommandRequest;
