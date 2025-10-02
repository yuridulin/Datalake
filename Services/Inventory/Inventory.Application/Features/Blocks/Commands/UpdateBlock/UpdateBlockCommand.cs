using Datalake.Inventory.Application.Features.Blocks.Models;
using Datalake.Inventory.Application.Interfaces;
using Datalake.Shared.Domain.Entities;

namespace Datalake.Inventory.Application.Features.Blocks.Commands.UpdateBlock;

public record UpdateBlockCommand(
	UserAccessEntity User,
	int BlockId,
	string Name,
	string? Description,
	IEnumerable<BlockTagDto> Tags) : ICommandRequest;
