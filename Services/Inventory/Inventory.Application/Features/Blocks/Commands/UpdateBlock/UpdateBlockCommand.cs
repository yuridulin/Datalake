using Datalake.Domain.ValueObjects;
using Datalake.Inventory.Application.Features.Blocks.Models;
using Datalake.Shared.Application.Interfaces;

namespace Datalake.Inventory.Application.Features.Blocks.Commands.UpdateBlock;

public record UpdateBlockCommand(
	UserAccessValue User,
	int BlockId,
	string Name,
	string? Description,
	IEnumerable<BlockTagDto> Tags) : ICommandRequest;
