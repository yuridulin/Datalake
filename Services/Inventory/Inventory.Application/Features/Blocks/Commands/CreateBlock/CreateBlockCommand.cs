using Datalake.Shared.Application.Entities;
using Datalake.Shared.Application.Interfaces;

namespace Datalake.Inventory.Application.Features.Blocks.Commands.CreateBlock;

public record CreateBlockCommand(
	UserAccessValue User,
	int? ParentId,
	string? Name,
	string? Description) : ICommandRequest;
