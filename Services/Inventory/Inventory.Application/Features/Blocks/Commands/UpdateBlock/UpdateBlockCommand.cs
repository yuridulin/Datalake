using Datalake.Inventory.Application.Features.Blocks.Models;
using Datalake.Shared.Application.Entities;
using Datalake.Shared.Application.Interfaces;

namespace Datalake.Inventory.Application.Features.Blocks.Commands.UpdateBlock;

public record UpdateBlockCommand(
	UserAccessEntity User,
	int BlockId,
	string Name,
	string? Description,
	IEnumerable<BlockTagDto> Tags) : ICommandRequest;
